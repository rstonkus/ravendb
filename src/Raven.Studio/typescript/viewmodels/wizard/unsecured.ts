import setupStep = require("viewmodels/wizard/setupStep");
import router = require("plugins/router");
import popoverUtils = require("common/popoverUtils");
import ipEntry = require("models/wizard/ipEntry");

class unsecured extends setupStep {

    canActivate(): JQueryPromise<canActivateResultDto> {
        const mode = this.model.mode();

        if (mode && mode === "Unsecured") {
            return $.when({ can: true });
        }

        return $.when({ redirect: "#welcome" });
    }

    attached() {
        super.attached();

        if (!this.model.unsecureSetup().ip()) {
            const initialIp = ipEntry.runningOnDocker ? "" : "127.0.0.1";
            
            this.model.unsecureSetup().ip(ipEntry.forIp(initialIp));                       
        }
    }    
    
    compositionComplete() {
        super.compositionComplete();
        
        popoverUtils.longWithHover($("label[for=serverUrl] .icon-info"),
            {
                content: 'The URL which the server should listen to. It can be hostname, ip address or 0.0.0.0:{port}',
            });
    }

    back() {
        router.navigate("#welcome");
    }
    
    save() {
        let isValid = true;

        if (!this.isValid(this.model.unsecureSetup().ip().validationGroup)) {
            isValid = false;
        }
        
        if (!this.isValid(this.model.unsecureSetup().validationGroup)) {
            isValid = false;
        }
        
        if (isValid) {
            router.navigate("#finish");
        }
    }

}

export = unsecured;
