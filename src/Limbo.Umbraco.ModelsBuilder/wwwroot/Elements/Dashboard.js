import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { LitElement, html, css, repeat, when, nothing, unsafeHTML } from "@umbraco-cms/backoffice/external/lit";

import { ModelsBuilderService } from "@limbo/models-builder/service";

const DELAY = 250;

async function delay(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
}

async function delayed(ms, callback) {
    const [, result] = await Promise.all([delay(ms), callback()]);
    return result;
}

export class LimboModelsBuilderDashboardElement extends UmbElementMixin(LitElement) {

    static properties = {
        loaded: { state: true },
        loading: { state: true },
        status: { state: true },
        reloadButtonState: { state: true },
        generateButtonState: { state: true },
    };

    #alerts = [];

    #dateOptions = {
        day: "numeric",
        hour: "numeric",
        minute: "numeric",
        month: "long",
        second: "numeric",
        year: "numeric"
    };

    constructor() {
        super();
        this.loaded = false;
        this.loading = false;
        this.status = null;
        this.reloadButtonState = null;
        this.generateButtonState = null;
        this.#updateStatus();
    }

    async #updateStatus() {

        this.loading = true;
        this.requestUpdate();

        try {

            const status = await delayed(DELAY, () => ModelsBuilderService.getStatus());

            this.status = {
                ...status,
                lastBuildDateFrom: this.#formatFromNow(status.lastBuildDate),
            };

            this.loaded = true;
            this.reloadButtonState = null;
            this.#updateUi();

        } finally {

            this.loading = false;
            this.#updateUi();

        }

    }

    #getAlerts() {

        const temp = [];

        if (!this.loaded) return temp;

        if (this.status?.environment !== "Development") {
            temp.push({ type: "danger", icon: "icon-alert", message: `Current environment is <strong>${this.status.environment}</strong>. It is not recommended building models in <strong>${this.status.environment}</strong> environments.` });
        }

        if (!this.status?.lastBuildDate) {
            temp.push({ type: "danger", icon: "icon-alert", message: "Models status is currently unknown." });
        } else if (this.status?.isOutOfDate) {
            temp.push({ type: "danger", icon: "icon-alert", message: "Models are <strong>out-of-date</strong>." });
        } else {
            temp.push({ type: "success", icon: "icon-check", message: "Models are <strong>up-to-date</strong>." });
        }

        return temp;

    }

    #updateUi() {
        this.#alerts = this.#getAlerts();
        this.requestUpdate();
    }

    async #reload() {
        this.reloadButtonState = "waiting";
        await this.#updateStatus();
    }

    async #generate() {

        this.loading = true;
        this.generateButtonState = "waiting";
        this.requestUpdate();

        try {
            const result = await delayed(DELAY, () => ModelsBuilderService.build());
            if (result.isSuccessful) {
                this.status = {
                    ...result,
                    lastBuildDateFrom: this.#formatFromNow(result.lastBuildDate),
                };
            }
            this.generateButtonState = null;
            this.#updateUi();
        } finally {
            this.loading = false;
            this.#updateUi();
        }

    }

    #formatFromNow(value) {
        if (!value) return "";
        const date = new Date(value);
        const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
        const rtf = new Intl.RelativeTimeFormat(this.localize.lang(), { numeric: "auto" });
        const abs = Math.abs(seconds);
        if (abs < 60) return rtf.format(-seconds, "second");
        if (abs < 3600) return rtf.format(-Math.round(seconds / 60), "minute");
        if (abs < 86400) return rtf.format(-Math.round(seconds / 3600), "hour");
        return rtf.format(-Math.round(seconds / 86400), "day");
    }

    #renderAlerts() {

        if (!this.#alerts?.length) return nothing;

        return html`
            ${repeat(this.#alerts, (alert) => html`
                <div class="alert alert-${alert.type}">
                    ${when(alert.icon, () => html`
                        <uui-icon name=${alert.icon}></uui-icon>
                    `)}
                    ${unsafeHTML(alert.message)}
                </div>
            `)}
        `;

    }

    #renderLinks() {
        const links = this.status?.links;
        if (!links?.length) return nothing;
        return html`
            <div class="links">
            ${links.map((link) => html`
                <uui-button look="secondary" label=${link.text} href=${link.url} target=${link.target || "_self"} rel=${link.rel || ""}>
                    <uui-icon name=${link.icon.replace("fa fa-", "icon-")}></uui-icon>
                    ${link.text}
                </uui-button>
                `
            )}
            </div>
        `;
    }

    render() {

        if (!this.loaded) return html`<uui-loader></uui-loader>`;

        return html`
            <uui-box headline="Limbo Models Builder" class="${this.loading ? "loading" : ""}">
                <div slot="header-actions">
                    <div class="actions">
                        <uui-button look="secondary" label="Reload" .state=${this.reloadButtonState} @click=${this.#reload}>Reload</uui-button>
                        <uui-button look="primary" color="positive" label="Generate" .state=${this.generateButtonState} @click=${this.#generate}></uui-button>
                    </div>
                </div>
                <div class="content">
                    ${this.#renderAlerts()}
                    <table>
                        <tr>
                            <th>Environment</th>
                            <td>${this.status?.environment || "N/A"}</td>
                        </tr>
                        <tr>
                            <th>Version</th>
                            <td>${this.status?.version || "N/A"}</td>
                        </tr>
                        <tr>
                            <th>Mode</th>
                            <td>
                                ${this.status?.mode || "N/A"}
                            </td>
                        </tr>
                        <tr>
                            <th>Models last generated</th>
                            <td>
                                ${when(this.status?.lastBuildDate, () => html`
                                    ${this.localize.date(this.status.lastBuildDate, this.#dateOptions)}
                                    ${when(this.status?.lastBuildDateFrom, () => html`
                                        <small>(${this.status?.lastBuildDateFrom})</small>
                                    `)}
                                `, () => html`
                                    <em>Never</em>
                                `)}
                            </td>
                        </tr>
                    </table>
                    ${this.#renderLinks()}
                </div>
            </uui-box>
        `;
    }

    static styles = css`

        :host {
            display: flex;
            flex-direction: column;
            height: 100%;
            min-height: 0;
        }

        uui-loader {
            position: absolute;
            left: 50%;
            top: 50%;
            margin: -6px 0 0 -6px;
            transform: translate(-50%, -50%);
        }

        .loading .content {
            opacity: 0.6;
        }

        uui-box {
            margin: 20px;
        }

        uui-icon {
            margin-right: 5px;
        }

        .content {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }

        table {
            border-spacing: 0;
        }

        th {
            min-width: 150px;
            width: 150px;
        }

        th, td {
            border-top: 1px solid var(--uui-color-border);
            text-align: left;
            padding: 10px;
        }

        tr:last-child th,
        tr:last-child td {
            border-bottom: 1px solid var(--uui-color-border);
        }

        .alert {
            padding: 10px;
            color: #fff;
            &.alert-success {
                background: var(--uui-color-positive-standalone);
            }
            &.alert-warning {
                background: var(--uui-color-warning-standalone);
            }
            &.alert-danger {
                background: var(--uui-color-danger-standalone);
            }
        }

    `;

}

customElements.define("limbo-models-builder-dashboard", LimboModelsBuilderDashboardElement);

export default LimboModelsBuilderDashboardElement;