export class ModelsBuilderPackage {

    static set serverVariables(value) {
        this._serverVariables = value;
    }

    static get version() {
        return this._serverVariables["version"];
    }

    static get cacheBuster() {
        return this._serverVariables["cacheBuster"];
    }

    static get settings() {
        return this._serverVariables["settings"];
    }

}

export default ModelsBuilderPackage;