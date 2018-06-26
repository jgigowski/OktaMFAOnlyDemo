using System.Web.Mvc;
using System.Web.Security;
using OktaAPIShared.Models;
using System.Web.Configuration;
using OktaCustomerUI.Helpers;
using OktaAPI.Helpers;

namespace OktaCustomerUI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // GET: /Account/Widget
        [AllowAnonymous]
        public ActionResult Widget()
        {
            ViewBag.OrgUrl = $"https://{WebConfigurationManager.AppSettings["okta:BaseUrl"]}/";
            ViewBag.OAuthRedirectUri = WebConfigurationManager.AppSettings["okta:OAuthRedirectUri"];
            ViewBag.OAuthIssuerId = WebConfigurationManager.AppSettings["okta:OAuthIssuerId"];
            ViewBag.OAuthClientId = WebConfigurationManager.AppSettings["okta:OAuthClientId"];

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string BasicLogin, string OIDCLogin)
        {
            return RedirectToAction("CustomLogin", model);
        }

        [AllowAnonymous]
        public ActionResult CustomLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            if (LoginHelper.IsValidLogin(model))
            {
                var bRequireMFA = true;//add logic to determine if the user should MFA if not all users are required

                if (bRequireMFA)
                {
                    // If MFA is required - Check to see if user has profile in Okta's Universal Directory (UD)
                    //     1) If user doesn't exist add profile and enroll them for MFA factor
                    //     2) if user exists check for Factor enrollment
                    //           a) if enrolled get factor and authenticate second factor
                    //           b) if not enrolled, enroll them for MFA factor

                    //shortcut - below is for supporting SMS only, not much more work involved in adding other factors

                    //https://developer.okta.com/docs/api/resources/factors#factors-api

                    var sUserId = APIHelper.GetUserIdByName(model);//Get or Add, return User
                    
                    if (!string.IsNullOrEmpty(sUserId))
                    {
                        //check for MFA factors
                        var oFactors = APIHelper.ListUserFactors(sUserId);

                        //specifically look for SMS - if more than one MFA used, get info from messafge to show dropdown and let user choose
                        var oSMSViewModel = new SMSViewModel();
                        oSMSViewModel.UserId = sUserId;

                        var sSMSFactorId = "";
                        foreach (var factor in oFactors)
                        {
                            if (factor.factorType == "sms")
                            {
                                sSMSFactorId = factor.id;
                            }
                        }

                        if (!string.IsNullOrEmpty(sSMSFactorId))//only setting up SMS for new users, so assume if there is one its SMS
                        {
                            //shortcut - should check if there is more than 1 and if so then prompt user to choose

                            //send sms verification automatically
                            var result = APIHelper.SendUserSMS(oSMSViewModel, sSMSFactorId);

                            if (result.errorSummary == null)
                            {
                                oSMSViewModel.ApiUrl = result._links.verify.href;

                                //put number in model because its required (not used for this flow), could clean up model to not require it as well
                                oSMSViewModel.CountryCode = "1"; //not used 
                                oSMSViewModel.PhoneNumber = "6302223333";//not used

                                return RedirectToAction("SMSVerify", "Account", oSMSViewModel);
                            }
                            else
                            {
                                TempData["Message"] = result.errorSummary.ToString() + " :("; //if you get a rate limit error, enhance to send 'resend'
                                TempData["IsError"] = true;
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            //no factors then just prompt to enroll for SMS
                            //shortcut - should check for which factors are available fpr the user to choose
                           
                            return RedirectToAction("SMSEnroll", "Account", oSMSViewModel);
                        }
                    }
                    else
                    {
                        //more than 1
                        TempData["Message"] = "No exact user match :(";
                        TempData["IsError"] = true;
                    }
                }
                TempData["Message"] = "Authenticated :)";
                TempData["IsError"] = false;
            }
            else
            {
                TempData["Message"] = "Invalid Login :(";
                TempData["IsError"] = true;
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult SMSVerify(SMSViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.PassCode))
            {
                var result = APIHelper.VerifyUserSMS(model);

                if (result.status == "ACTIVE" || result.factorResult == "SUCCESS")
                {
                    TempData["Message"] = "User Authenticated :)";
                    TempData["IsError"] = false;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Message"] = "Factor code didn't work :(";
                    TempData["IsError"] = true;
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult SMSEnroll(SMSViewModel model)
        {
            if (Request.RequestType == "GET")//initial get just displays the view
            {
                ModelState.Clear();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = APIHelper.EnrollUserSMS(model);

            if (result.errorSummary == null)
            {
                model.ApiUrl = result._links.activate.href;
                model.UserId = result.id;

                return RedirectToAction("SMSVerify", "Account", model);
            }

            TempData["Message"] = result.errorSummary.ToString() + " :(";
            TempData["IsError"] = true;
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }
    }
}