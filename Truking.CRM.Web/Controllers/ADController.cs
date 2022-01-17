using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Http;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Model;

namespace Truking.CRM.Web.Controllers
{
    public class ADController : TKApiController
    {
        [HttpPost]
        public WebRv ChangeMyPassword(AdModel model)
        {
            WebRv rv = new WebRv();
            try
            {
                string ldapPath = "LDAP://172.16.200.11";
                DirectoryEntry directionEntry = new DirectoryEntry(ldapPath, "truking" + "\\" + model.userName, model.currentPassword);
                if (directionEntry != null)

                {
                    DirectorySearcher search = new DirectorySearcher(directionEntry);
                    search.Filter = "(SAMAccountName=" + model.userName + ")";
                    SearchResult result = search.FindOne();
                    if (result != null)
                    {
                        DirectoryEntry userEntry = result.GetDirectoryEntry();
                        if (userEntry != null)
                        {
                            userEntry.Invoke("ChangePassword", new object[] { model.currentPassword, model.newPassword });
                            userEntry.CommitChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.failure;
                rv.msg = ex.Message;
            }
            return rv;
        }
    }
}