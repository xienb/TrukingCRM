﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.42000 版自动生成。
// 
#pragma warning disable 1591

namespace Truking.CRM.WinSrv.cn.truking.sappodev.rate {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="SI_EXCHANGE_RATE_OUTBinding", Namespace="urn:crm:exchange_rate")]
    public partial class SI_EXCHANGE_RATE_OUTService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback SI_EXCHANGE_RATE_OUTOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public SI_EXCHANGE_RATE_OUTService() {
            this.Url = global::Truking.CRM.WinSrv.Properties.Settings.Default.Truking_CRM_WinSrv_cn_truking_sappodev_rate_SI_EXCHANGE_RATE_OUTService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event SI_EXCHANGE_RATE_OUTCompletedEventHandler SI_EXCHANGE_RATE_OUTCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://sap.com/xi/WebService/soap1.1", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("MT_EXCHANGE_RATE_RSP", Namespace="urn:crm:exchange_rate")]
        public DT_EXCHANGE_RATE_RSP SI_EXCHANGE_RATE_OUT([System.Xml.Serialization.XmlElementAttribute(Namespace="urn:crm:exchange_rate")] DT_EXCHANGE_RATE_REQ MT_EXCHANGE_RATE_REQ) {
            object[] results = this.Invoke("SI_EXCHANGE_RATE_OUT", new object[] {
                        MT_EXCHANGE_RATE_REQ});
            return ((DT_EXCHANGE_RATE_RSP)(results[0]));
        }
        
        /// <remarks/>
        public void SI_EXCHANGE_RATE_OUTAsync(DT_EXCHANGE_RATE_REQ MT_EXCHANGE_RATE_REQ) {
            this.SI_EXCHANGE_RATE_OUTAsync(MT_EXCHANGE_RATE_REQ, null);
        }
        
        /// <remarks/>
        public void SI_EXCHANGE_RATE_OUTAsync(DT_EXCHANGE_RATE_REQ MT_EXCHANGE_RATE_REQ, object userState) {
            if ((this.SI_EXCHANGE_RATE_OUTOperationCompleted == null)) {
                this.SI_EXCHANGE_RATE_OUTOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSI_EXCHANGE_RATE_OUTOperationCompleted);
            }
            this.InvokeAsync("SI_EXCHANGE_RATE_OUT", new object[] {
                        MT_EXCHANGE_RATE_REQ}, this.SI_EXCHANGE_RATE_OUTOperationCompleted, userState);
        }
        
        private void OnSI_EXCHANGE_RATE_OUTOperationCompleted(object arg) {
            if ((this.SI_EXCHANGE_RATE_OUTCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SI_EXCHANGE_RATE_OUTCompleted(this, new SI_EXCHANGE_RATE_OUTCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:crm:exchange_rate")]
    public partial class DT_EXCHANGE_RATE_REQ {
        
        private string iT_DATAField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string IT_DATA {
            get {
                return this.iT_DATAField;
            }
            set {
                this.iT_DATAField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:crm:exchange_rate")]
    public partial class DT_EXCHANGE_RATE_RSP {
        
        private string eT_RETURNField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ET_RETURN {
            get {
                return this.eT_RETURNField;
            }
            set {
                this.eT_RETURNField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    public delegate void SI_EXCHANGE_RATE_OUTCompletedEventHandler(object sender, SI_EXCHANGE_RATE_OUTCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SI_EXCHANGE_RATE_OUTCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SI_EXCHANGE_RATE_OUTCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public DT_EXCHANGE_RATE_RSP Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((DT_EXCHANGE_RATE_RSP)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591