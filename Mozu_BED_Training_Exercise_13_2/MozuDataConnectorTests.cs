using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mozu.Api;
using Autofac;
using Mozu.Api.ToolKit.Config;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Mozu_BED_Training_Exercise_13_2
{
    [TestClass]
    public class MozuDataConnectorTests
    {
        private IApiContext _apiContext;
        private IContainer _container;

        [TestInitialize]
        public void Init()
        {
            _container = new Bootstrapper().Bootstrap().Container;
            var appSetting = _container.Resolve<IAppSetting>();
            var tenantId = int.Parse(appSetting.Settings["TenantId"].ToString());
            var siteId = int.Parse(appSetting.Settings["SiteId"].ToString());

            _apiContext = new ApiContext(tenantId, siteId);
        }

        [TestMethod]
        public void Exercise_13_1_Get_Customers()
        {
            //Create a Customer Account resource
            var customerAccountResource = new Mozu.Api.Resources.Commerce.Customer.CustomerAccountResource(_apiContext);

            //Retrieve an Account by id
            var account = customerAccountResource.GetAccountAsync(1001).Result;

            //Write the Account email
            System.Diagnostics.Debug.WriteLine("Account Email[{0}]: {1}", account.Id, account.EmailAddress);

            //You can also filter the Accounts Get call by email
            var accountByEmail = customerAccountResource.GetAccountsAsync(filter: "EmailAddress eq 'am_fake_email@whatafakeemail.com'").Result;

            //write account email
            System.Diagnostics.Debug.WriteLine("Account Email[{0}]: {1}", account.EmailAddress, account.Id);

            //Now, create a Customer Contact resource
            var customerContactResource = new Mozu.Api.Resources.Commerce.Customer.Accounts.CustomerContactResource(_apiContext);
            var customerContactCollection = new Mozu.Api.Contracts.Customer.CustomerContactCollection();
            if (accountByEmail.TotalCount > 0)
            {
                customerContactCollection = customerContactResource.GetAccountContactsAsync(accountByEmail.Items[0].Id).Result;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No contact information -- Customer does not exist");
            }

            if (customerContactCollection.TotalCount > 0)
            {
                foreach (var contact in customerContactCollection.Items)
                {
                    System.Diagnostics.Debug.WriteLine("Name:");
                    System.Diagnostics.Debug.WriteLine(contact.FirstName);
                    System.Diagnostics.Debug.WriteLine(contact.MiddleNameOrInitial);
                    System.Diagnostics.Debug.WriteLine(contact.LastNameOrSurname);
                    System.Diagnostics.Debug.WriteLine("Address:");
                    System.Diagnostics.Debug.WriteLine(contact.Address.Address1);
                    System.Diagnostics.Debug.WriteLine(contact.Address.Address2);
                    System.Diagnostics.Debug.WriteLine(contact.Address.Address3);
                    System.Diagnostics.Debug.WriteLine(contact.Address.Address4);
                    System.Diagnostics.Debug.WriteLine(contact.Address.CityOrTown);
                    System.Diagnostics.Debug.WriteLine(contact.Address.StateOrProvince);
                    System.Diagnostics.Debug.WriteLine(contact.Address.PostalOrZipCode);
                    System.Diagnostics.Debug.WriteLine(contact.Address.CountryCode);
                    System.Diagnostics.Debug.WriteLine(String.Format("Is a validated address? {0}", contact.Address.IsValidated));
                }
            }

            //Create a Customer Credit resource
            var creditResource = new Mozu.Api.Resources.Commerce.Customer.CreditResource(_apiContext);

            //Get credits by customer account id
            var customerCredits = creditResource.GetCreditsAsync(filter: "CustomerId eq '1001'").Result;

            foreach (var customerCredit in customerCredits.Items)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Customer Credit[{0}]: Code({1})Balance ({2})", customerCredit.CustomerId, customerCredit.Code, customerCredit.CurrentBalance));
            }
        }

        [TestMethod]
        public void Exercise_13_2_Add_New_Customer()
        {
            //Create a Customer Account resource
            var customerAccountResource = new Mozu.Api.Resources.Commerce.Customer.CustomerAccountResource(_apiContext);

            var existingAcct = customerAccountResource.GetAccountsAsync(filter: "EmailAddress eq 'captainmal@serenitycorp.com'").Result;

            if (existingAcct == null || existingAcct.TotalCount == 0)
            {
                //Create a new Account Info and Authorization Info contract
                var customerAccountAndAuthInfo = new Mozu.Api.Contracts.Customer.CustomerAccountAndAuthInfo()
                {
                    Account = new Mozu.Api.Contracts.Customer.CustomerAccount()
                    {
                        AcceptsMarketing = false,
                        CompanyOrOrganization = "Serenity Corp.",
                        EmailAddress = "captainmal@serenitycorp.com",
                        ExternalId = "A0001",
                        FirstName = "Malcolm",
                        LastName = "Reynolds",
                        IsActive = true,
                        IsAnonymous = false,
                        LocaleCode = "en-US",
                        TaxExempt = false,
                        IsLocked = false,
                        UserName = "captainmal@serenitycorp.com",
                    },
                    Password = "Password1",
                    IsImport = true
                };

                var newAccount = customerAccountResource.AddAccountAndLoginAsync(customerAccountAndAuthInfo).Result;

                var contactMal = new Mozu.Api.Contracts.Customer.CustomerContact()
                {
                    Email = "captainmal@serenitycorp.com",
                    FirstName = "Malcolm",
                    LastNameOrSurname = "Reynolds",
                    Label = "Capt.",
                    PhoneNumbers = new Mozu.Api.Contracts.Core.Phone()
                    {
                        Mobile = "555-555-0001"
                    },
                    Address = new Mozu.Api.Contracts.Core.Address()
                    {
                        Address1 = "03-K64 Firefly Transport",
                        AddressType = "Residential",
                        CityOrTown = "Austin",
                        CountryCode = "US",
                        PostalOrZipCode = "78759",
                        StateOrProvince = "TX"
                    },
                    Types = new System.Collections.Generic.List<Mozu.Api.Contracts.Customer.ContactType>()
                    {
                        new Mozu.Api.Contracts.Customer.ContactType()
                        {
                            IsPrimary = true,
                                Name = "Billing"
                        }
                    }
                };

                var contactInara = new Mozu.Api.Contracts.Customer.CustomerContact()
                {
                    Email = "inara@serenitycorp.com",
                    FirstName = "Inara",
                    LastNameOrSurname = "Serra",
                    Label = "Ms.",
                    PhoneNumbers = new Mozu.Api.Contracts.Core.Phone()
                    {
                        Mobile = "555-555-0002"
                    },
                    Address = new Mozu.Api.Contracts.Core.Address()
                    {
                        Address1 = "03-K64 Firefly Transport -- Shuttle",
                        AddressType = "Residential",
                        CityOrTown = "Austin",
                        CountryCode = "US",
                        PostalOrZipCode = "78759",
                        StateOrProvince = "TX"
                    },
                    Types = new System.Collections.Generic.List<Mozu.Api.Contracts.Customer.ContactType>()
                    {
                        new Mozu.Api.Contracts.Customer.ContactType()
                        {
                            IsPrimary = false,
                                Name = "Billing"
                        }
                    }
                };

                //Create a Customer Contact resource
                var contactResource = new Mozu.Api.Resources.Commerce.Customer.Accounts.CustomerContactResource(_apiContext);

                //Add new contact
                var newContactMal = contactResource.AddAccountContactAsync(contactMal, newAccount.CustomerAccount.Id).Result;

                //Add additional contact
                var newContactInara = contactResource.AddAccountContactAsync(contactInara, newAccount.CustomerAccount.Id).Result;
            }
                //Create a Customer Credit resource
                var creditResource = new Mozu.Api.Resources.Commerce.Customer.CreditResource(_apiContext);

                //Create a Credit object
                var credit = new Mozu.Api.Contracts.Customer.Credit.Credit()
                {
                    ActivationDate = DateTime.Now,
                    Code = Guid.NewGuid().ToString("N"),
                    CreditType = "GiftCard",
                    CurrencyCode = "USD",
                    CurrentBalance = 1000,
                    CustomerId = 1002,
                    InitialBalance = 1000
                };

                //Add credit
                var newCredit = creditResource.AddCreditAsync(credit).Result;
            
        }
    }
}
