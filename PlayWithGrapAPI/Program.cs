using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;

namespace PlayWithGrapAPI
{
    internal class Constants
    {
        public const string TenantName = "WebApiAuth07162016.onmicrosoft.com";
        public const string TenantId = "ff980802-7a57-4e31-bb55-7d88646adc31";
        public const string ClientId = "dd6668ff-6bc7-44e4-88d5-d7e8dee24ff2"; //WebApiTest
        public const string ClientSecret = "RzgYgaNQe1ZrqOxQagiA3RdJvZYI/XioWd+C7VSMry0=";
        public const string ClientIdForUserAuthn = "56d20922-acd2-4d18-a6d1-30c0119e70bb";  // authenticate to client app
        public const string AuthString = "https://login.microsoftonline.com/" + TenantName;
        public const string ResourceUrl = "https://graph.windows.net";
    }
    class Program
    {
        static void Main(string[] args)
        {
            string currentDateTime = DateTime.Now.ToUniversalTime().ToString();

            ActiveDirectoryClient activeDirectoryClient;
            try
            {
                activeDirectoryClient = AuthenticationHelper.GetActiveDirectoryClientAsApplication();
            }
            catch (AuthenticationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Acquiring a token failed with the following error: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    //You should implement retry and back-off logic per the guidance given here:http://msdn.microsoft.com/en-us/library/dn168916.aspx
                    //InnerException Message will contain the HTTP error status codes mentioned in the link above
                    Console.WriteLine("Error detail: {0}", ex.InnerException.Message);
                }
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            #region tennant deatail
            VerifiedDomain initialDomain = new VerifiedDomain();
            VerifiedDomain defaultDomain = new VerifiedDomain();
            ITenantDetail tenant = null;
            Console.WriteLine("\n Retrieving Tenant Details");
            try
            {
                List<ITenantDetail> tenantsList = activeDirectoryClient.TenantDetails
                    .Where(tenantDetail => tenantDetail.ObjectId.Equals(Constants.TenantId))
                    .ExecuteAsync().Result.CurrentPage.ToList();
                if (tenantsList.Count > 0)
                {
                    tenant = tenantsList.First();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting TenantDetails {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }

            if (tenant == null)
            {
                Console.WriteLine("Tenant not found");
            }
            else
            {
                TenantDetail tenantDetail = (TenantDetail)tenant;
                Console.WriteLine("Tenant Display Name: " + tenantDetail.DisplayName);

                // Get the Tenant's Verified Domains 
                initialDomain = tenantDetail.VerifiedDomains.First(x => x.Initial.HasValue && x.Initial.Value);
                Console.WriteLine("Initial Domain Name: " + initialDomain.Name);
                defaultDomain = tenantDetail.VerifiedDomains.First(x => x.@default.HasValue && x.@default.Value);
                Console.WriteLine("Default Domain Name: " + defaultDomain.Name);

                // Get Tenant's Tech Contacts
                foreach (string techContact in tenantDetail.TechnicalNotificationMails)
                {
                    Console.WriteLine("Tenant Tech Contact: " + techContact);
                }
            }
            #endregion

            #region Create a new User

            //IUser newUser = new User();
            //if (defaultDomain.Name != null)
            //{
            //    newUser.DisplayName = "Sample App Demo User (Manager)";
            //    newUser.UserPrincipalName = Helper.GetRandomString(10) + "@" + defaultDomain.Name;
            //    newUser.AccountEnabled = true;
            //    newUser.MailNickname = "SampleAppDemoUserManager";
            //    newUser.PasswordProfile = new PasswordProfile
            //    {
            //        Password = "TempP@ssw0rd!",
            //        ForceChangePasswordNextLogin = true
            //    };
            //    newUser.UsageLocation = "US";

            //    try
            //    {
            //        activeDirectoryClient.Users.AddUserAsync(newUser).Wait();
            //        Console.WriteLine("\nNew User {0} was created", newUser.DisplayName);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("\nError creating new user {0} {1}", e.Message,
            //            e.InnerException != null ? e.InnerException.Message : "");
            //    }
            //}

            #endregion

            #region List of max 4 Users by UPN

            //*********************************************************************
            // Demonstrate Getting a list of Users with paging (get 4 users), sorted by displayName
            //*********************************************************************
            //int maxUsers = 4;
            //try
            //{
            //    Console.WriteLine("\n Retrieving Users");
            //    List<IUser> users = activeDirectoryClient.Users.OrderBy(user =>
            //        user.UserPrincipalName).Take(10).ExecuteAsync().Result.CurrentPage.ToList();
            //    foreach (IUser user in users)
            //    {
            //        Console.WriteLine("UserObjectId: {0}  UPN: {1}", user.ObjectId, user.UserPrincipalName);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\nError getting Users. {0} {1}", e.Message,
            //        e.InnerException != null ? e.InnerException.Message : "");
            //}

            #endregion

            #region Search User by UPN

            // search for a single user by UPN
            string searchString1 = "user1@" + initialDomain.Name;
            string searchString2 = "user2@" + initialDomain.Name;
            Console.WriteLine("\n Retrieving user with UPN {0}", searchString1);
            User retrievedUser = new User();
            User newUser = new User();
            List<IUser> retrievedUsers = null;
            try
            {
                retrievedUsers = activeDirectoryClient.Users
                    .Where(user => user.UserPrincipalName.Equals(searchString1))
                    .ExecuteAsync().Result.CurrentPage.ToList();


            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting new user {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }
            // should only find one user with the specified UPN
            if (retrievedUsers != null && retrievedUsers.Count == 1)
            {
                retrievedUser = (User)retrievedUsers.First();

                retrievedUsers = activeDirectoryClient.Users
                   .Where(user => user.UserPrincipalName.Equals(searchString2))
                   .ExecuteAsync().Result.CurrentPage.ToList();
                newUser = (User)retrievedUsers.First();
            }
            else
            {
                Console.WriteLine("User not found {0}", searchString1);
            }

            //#region User Operations

            //if (retrievedUser.UserPrincipalName != null)
            //{
            //    Console.WriteLine("\n Found User: " + retrievedUser.DisplayName + "  UPN: " +
            //                      retrievedUser.UserPrincipalName);

            //    #region Assign User a Manager

            //    //Assigning User a new manager.
            //    if (newUser.ObjectId != null)
            //    {
            //        Console.WriteLine("\n Assign User {0}, {1} as Manager.", retrievedUser.DisplayName,
            //            newUser.DisplayName);
            //        retrievedUser.Manager = newUser as DirectoryObject;
            //        try
            //        {
            //            newUser.UpdateAsync().Wait();
            //            Console.Write("User {0} is successfully assigned {1} as Manager.", retrievedUser.DisplayName,
            //                newUser.DisplayName);
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine("\nError assigning manager to user. {0} {1}", e.Message,
            //                e.InnerException != null ? e.InnerException.Message : "");
            //        }
            //    }

            //    #endregion

            //    #region Get User's Manager

            //    //Get the retrieved user's manager.
            //    Console.WriteLine("\n Retrieving User {0}'s Manager.", retrievedUser.DisplayName);
            //    DirectoryObject usersManager = retrievedUser.Manager;
            //    if (usersManager != null)
            //    {
            //        User manager = usersManager as User;
            //        if (manager != null)
            //        {
            //            Console.WriteLine("User {0} Manager details: \nManager: {1}  UPN: {2}",
            //                retrievedUser.DisplayName, manager.DisplayName, manager.UserPrincipalName);
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("Manager not found.");
            //    }

            //    #endregion

            //    #region Get User's Direct Reports

            //    //*********************************************************************
            //    // get the user's Direct Reports
            //    //*********************************************************************
            //    if (newUser.ObjectId != null)
            //    {
            //        Console.WriteLine("\n Getting User{0}'s Direct Reports.", newUser.DisplayName);
            //        IUserFetcher newUserFetcher = (IUserFetcher)newUser;
            //        try
            //        {
            //            IPagedCollection<IDirectoryObject> directReports =
            //                newUserFetcher.DirectReports.ExecuteAsync().Result;
            //            do
            //            {
            //                List<IDirectoryObject> directoryObjects = directReports.CurrentPage.ToList();
            //                foreach (IDirectoryObject directoryObject in directoryObjects)
            //                {
            //                    if (directoryObject is User)
            //                    {
            //                        User directReport = directoryObject as User;
            //                        Console.WriteLine("User {0} Direct Report is {1}", newUser.UserPrincipalName,
            //                            directReport.UserPrincipalName);
            //                    }
            //                }
            //                directReports = directReports.GetNextPageAsync().Result;
            //            } while (directReports != null);
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine("\nError getting direct reports of user. {0} {1}", e.Message,
            //                e.InnerException != null ? e.InnerException.Message : "");
            //        }
            //    }

            //    #endregion

            //    #region Get list of Group IDS, user is member of

            //    //*********************************************************************
            //    // get a list of Group IDs that the user is a member of
            //    //*********************************************************************
            //    //const bool securityEnabledOnly = false;
            //    //IEnumerable<string> memberGroups = retrievedUser.GetMemberGroupsAsync(securityEnabledOnly).Result;
            //    //Console.WriteLine("\n {0} is a member of the following Groups (IDs)", retrievedUser.DisplayName);
            //    //foreach (String memberGroup in memberGroups)
            //    //{
            //    //    Console.WriteLine("Member of Group ID: " + memberGroup);
            //    //}

            //    #endregion

            //    #region Get User's Group And Role Membership, Getting the complete set of objects

            //    //*********************************************************************
            //    // get the User's Group and Role membership, getting the complete set of objects
            //    //*********************************************************************
            //    Console.WriteLine("\n {0} is a member of the following Group and Roles (IDs)", retrievedUser.DisplayName);
            //    IUserFetcher retrievedUserFetcher = retrievedUser;
            //    try
            //    {
            //        IPagedCollection<IDirectoryObject> pagedCollection = retrievedUserFetcher.MemberOf.ExecuteAsync().Result;
            //        do
            //        {
            //            List<IDirectoryObject> directoryObjects = pagedCollection.CurrentPage.ToList();
            //            foreach (IDirectoryObject directoryObject in directoryObjects)
            //            {
            //                if (directoryObject is Group)
            //                {
            //                    Group group = directoryObject as Group;
            //                    Console.WriteLine(" Group: {0}  Description: {1}", group.DisplayName, group.Description);
            //                }
            //                if (directoryObject is DirectoryRole)
            //                {
            //                    DirectoryRole role = directoryObject as DirectoryRole;
            //                    Console.WriteLine(" Role: {0}  Description: {1}", role.DisplayName, role.Description);
            //                }
            //            }
            //            pagedCollection = pagedCollection.GetNextPageAsync().Result;
            //        } while (pagedCollection != null);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("\nError getting user's groups and roles memberships. {0} {1}", e.Message,
            //            e.InnerException != null ? e.InnerException.Message : "");
            //    }

            //    #endregion
            //}

            #endregion

            // #endregion

            #region Get All Roles

            ////*********************************************************************
            //// Get All Roles
            ////*********************************************************************
            //List<IDirectoryRole> foundRoles = null;
            //try
            //{
            //    foundRoles = activeDirectoryClient.DirectoryRoles.ExecuteAsync().Result.CurrentPage.ToList();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\nError getting Roles {0} {1}", e.Message,
            //        e.InnerException != null ? e.InnerException.Message : "");
            //}

            //if (foundRoles != null && foundRoles.Count > 0)
            //{
            //    foreach (IDirectoryRole role in foundRoles)
            //    {
            //        Console.WriteLine("\n Found Role: {0} {1} {2} ",
            //            role.DisplayName, role.Description, role.ObjectId);
            //    }
            //}
            //else
            //{
            //    //Console.WriteLine("Role Not Found {0}", searchString);
            //}

            #endregion

            #region Get Service Principals

            //*********************************************************************
            // get the Service Principals
            //*********************************************************************
            //IPagedCollection<IServicePrincipal> servicePrincipals = null;
            //try
            //{
            //    servicePrincipals = activeDirectoryClient.ServicePrincipals.ExecuteAsync().Result;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\nError getting Service Principal {0} {1}",
            //        e.Message, e.InnerException != null ? e.InnerException.Message : "");
            //}
            //if (servicePrincipals != null)
            //{
            //    do
            //    {
            //        List<IServicePrincipal> servicePrincipalsList = servicePrincipals.CurrentPage.ToList();
            //        foreach (IServicePrincipal servicePrincipal in servicePrincipalsList)
            //        {
            //            Console.WriteLine("Service Principal AppId: {0}  Name: {1}", servicePrincipal.AppId,
            //                servicePrincipal.DisplayName);
            //        }
            //        servicePrincipals = servicePrincipals.GetNextPageAsync().Result;
            //    } while (servicePrincipals != null);
            //}

            #endregion

            #region Get Applications

            //*********************************************************************
            // get the Application objects
            //*********************************************************************
            IPagedCollection<IApplication> applications = null;
            try
            {
                applications = activeDirectoryClient.Applications.Take(999).ExecuteAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting Applications {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }
            if (applications != null)
            {
                do
                {
                    List<IApplication> appsList = applications.CurrentPage.ToList();
                    foreach (IApplication app in appsList)
                    {
                        Console.WriteLine("Application AppId: {0}  Name: {1}", app.AppId, app.DisplayName);
                    }
                    applications = applications.GetNextPageAsync().Result;
                } while (applications != null);
            }

            #endregion

            #region Switch to OAuth Authorization Code Grant (Acting as a user)

            activeDirectoryClient = AuthenticationHelper.GetActiveDirectoryClientAsUser();

            #endregion

            #region Create Application

            //*********************************************************************************************
            // Create a new Application object with App Role Assignment (Direct permission)
            //*********************************************************************************************
            //Application appObject = new Application { DisplayName = "Test-Demo App" + Helper.GetRandomString(8) };
            //appObject.IdentifierUris.Add("https://localhost/demo/" + Guid.NewGuid());
            //appObject.ReplyUrls.Add("https://localhost/demo");
            //AppRole appRole = new AppRole();
            //appRole.Id = Guid.NewGuid();
            //appRole.IsEnabled = true;
            //appRole.AllowedMemberTypes.Add("User");
            //appRole.DisplayName = "Something";
            //appRole.Description = "Anything";
            //appRole.Value = "policy.write";
            //appObject.AppRoles.Add(appRole);

            //// created Keycredential object for the new App object
            //KeyCredential keyCredential = new KeyCredential
            //{
            //    StartDate = DateTime.UtcNow,
            //    EndDate = DateTime.UtcNow.AddYears(1),
            //    Type = "Symmetric",
            //    Value = Convert.FromBase64String("g/TMLuxgzurjQ0Sal9wFEzpaX/sI0vBP3IBUE/H/NS4="),
            //    Usage = "Verify"
            //};
            //appObject.KeyCredentials.Add(keyCredential);

            //try
            //{
            //    activeDirectoryClient.Applications.AddApplicationAsync(appObject).Wait();
            //    Console.WriteLine("New Application created: " + appObject.ObjectId);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Application Creation execption: {0} {1}", e.Message,
            //        e.InnerException != null ? e.InnerException.Message : "");
            //}

            #endregion

            #region Create Service Principal

            //*********************************************************************************************
            // create a new Service principal
            ////*********************************************************************************************
            //ServicePrincipal newServicePrincpal = new ServicePrincipal();
            //if (appObject != null)
            //{
            //    newServicePrincpal.DisplayName = appObject.DisplayName;
            //    newServicePrincpal.AccountEnabled = true;
            //    newServicePrincpal.AppId = appObject.AppId;
            //    try
            //    {
            //        activeDirectoryClient.ServicePrincipals.AddServicePrincipalAsync(newServicePrincpal).Wait();
            //        Console.WriteLine("New Service Principal created: " + newServicePrincpal.ObjectId);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("Service Principal Creation execption: {0} {1}", e.Message,
            //            e.InnerException != null ? e.InnerException.Message : "");
            //    }
            //}

            IApplicationFetcher appObject1 = activeDirectoryClient.Applications.GetByObjectId("e36f5c4e-c30b-4854-a295-3fcbdf04e6c0");
            Application appObject = (Application)(appObject1.ExecuteAsync().Result);
            //ExtensionProperty linkedInUserId = new ExtensionProperty
            //{
            //    Name = "linkedInUserId",
            //    DataType = "String",
            //    TargetObjects = { "User" }
            //};
            //try
            //{
            //    appObject.ExtensionProperties.Add(linkedInUserId);
            //    appObject.UpdateAsync().Wait();
            //    appObject.GetContext().SaveChanges();
            //    Console.WriteLine("\nUser object extended successfully.");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\nError extending the user object {0} {1}", e.Message,
            //        e.InnerException != null ? e.InnerException.Message : "");
            // }

            //IEnumerable<IExtensionProperty> allExtprop=  activeDirectoryClient.GetAvailableExtensionPropertiesAsync(false).Result;

            //try
            //{
            //    if (retrievedUser != null && retrievedUser.ObjectId != null)
            //    {
            //        retrievedUser.SetExtendedProperty("extension"+"_"+appObject.AppId.Replace("-", "") + "_"+ "linkedInUserId", "ExtensionPropertyValue");
            //        retrievedUser.UpdateAsync().Wait();
            //        Console.WriteLine("\nUser {0}'s extended property set successully.", retrievedUser.DisplayName);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\nError Updating the user object {0} {1}", e.Message,
            //        e.InnerException != null ? e.InnerException.Message : "");
            //}

            //#endregion

            //#region Get an Extension Property

            //try
            //{
            //    if (retrievedUser != null && retrievedUser.ObjectId != null)
            //    {
            //        IReadOnlyDictionary<string, object> extendedProperties = retrievedUser.GetExtendedProperties();
            //        object extendedProperty = extendedProperties[appObject.ExtensionProperties[0].Name];
            //        Console.WriteLine("\n Retrieved User {0}'s extended property value is: {1}.", retrievedUser.DisplayName,
            //            extendedProperty);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("\nError Updating the user object {0} {1}", e.Message,
            //        e.InnerException != null ? e.InnerException.Message : "");
            //}
            #endregion
            try
            {
                User user =
                        (User)activeDirectoryClient.Users.ExecuteAsync().Result.CurrentPage.ToList().FirstOrDefault();
                if (appObject.ObjectId != null && user != null && newServicePrincpal.ObjectId != null)
                {
                    AppRoleAssignment appRoleAssignment = new AppRoleAssignment();
                    appRoleAssignment.Id = appRole.Id;
                    appRoleAssignment.ResourceId = Guid.Parse(newServicePrincpal.ObjectId);
                    appRoleAssignment.PrincipalType = "User";
                    appRoleAssignment.PrincipalId = Guid.Parse(user.ObjectId);
                    user.AppRoleAssignments.Add(appRoleAssignment);
                    user.UpdateAsync().Wait();
                    Console.WriteLine("User {0} is successfully assigned direct permission.", retrievedUser.DisplayName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Direct Permission Assignment failed: {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }
        }
    }

    internal class Helper
    {
        /// <summary>
        ///     Returns a random string of upto 32 characters.
        /// </summary>
        /// <returns>String of upto 32 characters.</returns>
        public static string GetRandomString(int length = 32)
        {
            //because GUID can't be longer than 32
            return Guid.NewGuid().ToString("N").Substring(0, length > 32 ? 32 : length);
        }
    }
}
