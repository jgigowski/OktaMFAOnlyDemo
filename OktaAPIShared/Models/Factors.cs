using System;
using System.Runtime.Serialization;

namespace OktaAPIShared.Models
{
    [DataContract]
    public class EnrollSMS
    {
        [DataMember(Name = "factorType")]
        public String FactorType { get; set; }

        [DataMember(Name = "provider")]
        public String Provider { get; set; }

        [DataMember(Name = "profile")]
        public SMSProfile Profile { get; set; }

        public EnrollSMS()
        {
            FactorType = "sms";
            Provider = "OKTA";
            Profile = new SMSProfile();
        }
    }

    [DataContract]
    public class SMSProfile
    {
        [DataMember(Name = "phoneNumber")]
        public String PhoneNumber { get; set; } //Format - "+1-555-415-1337"
    }

    [DataContract]
    public class ActivateVerifySMS
    {
        [DataMember(Name = "userId")]
        public String UserId { get; set; }

        [DataMember(Name = "factorId")]
        public String FactorId { get; set; }

        [DataMember(Name = "passCode")]
        public String PassCode { get; set; }
    }

    [DataContract]
    public class SendSMSPassCode
    {
        [DataMember(Name = "passCode")]
        public String PassCode { get; set; }
    }
}