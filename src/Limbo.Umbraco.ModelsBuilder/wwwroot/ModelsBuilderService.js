import { ModelsBuilderAuth } from "@limbo/models-builder/auth";

async function hi(url, config) {

    if (!config) config = {};
    if (!config.method) config.method = "GET";
    if (!config.headers) config.headers = {};

    const token = await ModelsBuilderAuth.TOKEN();
    config.headers.Authorization = `Bearer ${token}`;

    const res = await fetch(url, config);

    const contentType = res.headers.get("content-type") || "";

    if (contentType.includes("application/json")) {
        res.data = await res.json();
    } else if (contentType.startsWith("text/")) {
        res.textContent = await res.text();
    } else {
        throw new Error(`Unsupported content type: ${contentType}`);
    }

    if (!res.ok) {
        throw res;
    }

    return res;

}

async function get(url) {
    return await hi(url);
}

async function patch(url, config) {
    if (!config) config = {};
    config.method = "PATCH";
    return await hi(url, config);
}

async function patchJson(url, body, config) {
    if (!config) config = {};
    if (!config.headers) config.headers = {};
    config.headers["Content-Type"] = "application/json";
    config.body = JSON.stringify(body);
    return await patch(url, config);
}

async function put(url, config) {
    if (!config) config = {};
    config.method = "PUT";
    return await hi(url, config);
}

async function putJson(url, body, config) {
    if (!config) config = {};
    if (!config.headers) config.headers = {};
    config.headers["Content-Type"] = "application/json";
    config.body = JSON.stringify(body);
    return await put(url, config);
}

async function _delete(url, config) {
    if (!config) config = {};
    config.method = "DELETE";
    return await hi(url, config);
}

const baseUrl = "/umbraco/management/api/v1/limbo/modelsbuilder";

export class ModelsBuilderService {

    static async getServerVariables() {
        const response = await get(`${baseUrl}/serverVariables`);
        return response.data;
    }

    static async getStatus() {
        const response = await get(`${baseUrl}/status`);
        return response.data;
    }

    static async build() {
        const response = await get(`${baseUrl}/build`);
        return response.data;
    }

}

export default ModelsBuilderService;