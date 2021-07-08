using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace QSC_Test_Automation
{

    public class QRCM_API
    {
        private string QRCMLogonTokenvalue = string.Empty;
        public string QRCMLogonToken
        {
            get { return QRCMLogonTokenvalue; }
            set { QRCMLogonTokenvalue = value; }
        }

        public Tuple<bool, string> QRCM_HTTPAction(string methodname, string token, string ipaddress, List<string> args, string userpayload)
        {
            if (userpayload == "None")
                userpayload = string.Empty;
            string url = string.Empty;
            string method_type = string.Empty;
            string payload = string.Empty;
            string fileContentName = string.Empty;
            Tuple<bool, string> outputdata = new Tuple<bool, string>(false, string.Empty);
            Dictionary<string, string> combine = new Dictionary<string, string>();
            bool no_Such_Method = false;
            try
            {
                switch (methodname)
                {

                    case ("rename_system"): { url = "/api/v0/systems/1"; method_type = "PUT"; payload = "{\"name\":\"" + args[0] + "\"}"; break; }
                    case ("clear_events"): { url = "/api/v0/systems/1/events"; method_type = "DELETE"; break; }
                    case ("create_new_user"):
                        {
                            url = "/api/v0/cores/self/users"; method_type = "POST";
                            payload = "{\"role\": \"" + args[0] + "\", \"username\": \"" + args[1] + "\", \"password\": \"" + args[2] + "\",\"passwordConfirm\": \"" + args[3] + "\"}";
                            break;
                        }
                    case ("update_user_details"):
                        {
                            url = "/api/v0/cores/self/users/" + args[0] + ""; method_type = "PUT";
                            payload = "{\"id\": \"" + args[0] + "\",\"role\": \"" + args[2] + "\", \"username\": \"" + args[1] + "\"}";
                            break;
                        }
                    case ("update_user_password"):
                        {
                            url = "/api/v0/cores/self/users/" + args[0] + "/password"; method_type = "PUT";
                            payload = "{\"password\": \"" + args[1] + "\",\"passwordConfirm\": \"" + args[2] + "\"}";
                            break;
                        }
                    case ("delete_user"):
                        {
                            url = "/api/v0/cores/self/users/" + args[0] + ""; method_type = "DELETE";
                            break;
                        }
                    case ("delete_custom_role"):
                        {
                            url = "/api/v0/cores/self/roles/" + args[0] + ""; method_type = "DELETE";
                            break;
                        }
                    case ("network_settings_update"):
                        {
                            url = "/api/v0/cores/self/config/network"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("disable_access_control"):
                        {
                            url = "/api/v0/cores/self/access_mode"; method_type = "PUT";
                            payload = "{\"accessMode\": \"open\", \"removeUsers\": \"true\"}";
                            break;
                        }
                    case ("audio_files_create_directory"):
                        {
                            if (args[0].Contains('/'))
                            {
                                string argval = args[0];
                                if (argval.EndsWith("/"))
                                {
                                    argval = argval.Remove(argval.Length - 1);
                                }

                                string[] url_construct = argval.Split('/');
                                payload = "{\"name\": \"" + url_construct.Last() + "\"}";
                                url = "/api/v0/cores/self/media/" + string.Join("/", url_construct.Take(url_construct.Length - 1));
                                method_type = "POST";
                            }
                            else
                            {
                                url = "/api/v0/cores/self/media"; method_type = "POST";
                                payload = "{\"name\": \"" + args[0] + "\"}";
                            }
                                                       
                            break;
                        }
                    case ("audio_files_rename_file_directory"):
                        {
                            url = "/api/v0/cores/self/media"; method_type = "PUT";
                            payload = "[{\"" + args[0] + "\": { \"path\": \"" + args[1] + "\"}}]";
                            break;
                        }
                    case ("audio_files_delete_file_directory"):
                        {
                            url = "/api/v0/cores/self/media/"; method_type = "DELETE";
                            payload = "[\""+args[0]+"\"]";
                            break;
                        }
                    case ("audio_files_create_playlist"):
                        {
                            url = "/api/v0/cores/self/media_playlists"; method_type = "POST";
                            payload = "{\"name\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("audio_files_add_file_to_playlist"):
                        {
                            url = "/api/v0/cores/self/media_playlists/" + args[0] + "/media"; method_type = "POST";
                            payload = "[{\"path\":\"" + args[1] + "\"}]";
                            break;
                        }
                    case ("reboot"):
                        {
                            url = "/api/v0/cores/self/config/reboot"; method_type = "PUT";
                            break;
                        }
                    case ("add_uci_pin"):
                        {
                            url = "/api/v0/systems/1/ucis/pins"; method_type = "POST";
                            payload = "[{\"name\":\"" + args[0] + "\",\"pin\":\"" + args[1] + "\"}]";
                            break;
                        }
                    case ("modify_uci_pin"):
                        {
                            url = "/api/v0/systems/1/ucis/pins"; method_type = "PUT";
                            payload = "[{\"id\":\"" + args[0] + "\",\"name\":\"" + args[1] + "\",\"pin\":\"" + args[2] + "\"}]";
                            break;
                        }
                    case ("delete_uci_pin"):
                        {
                            url = "/api/v0/systems/1/ucis/pins?id=" + args[0] + ""; method_type = "DELETE";
                            break;
                        }
                    case ("add_pin_to_uci"):
                        {
                            url = "/api/v0/systems/1/ucis"; method_type = "PUT";
                            payload = "{\"fileName\":\"" + args[0] + "\",\"pinIds\":[\"" + args[1] + "\"]}";
                            break;
                        }

                    case ("update_contact_book"):
                        {
                            url = "/api/v0/systems/1/contact_books/" + args[0] + ""; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("delete_contact_book"):
                        {
                            url = "/api/v0/systems/1/contact_books/" + args[0] + ""; method_type = "DELETE";
                            break;
                        }
                    case ("update_camera_settings"):
                        {
                            url = "/api/v0/systems/1/cameras/settings"; method_type = "PUT";
                            payload = "{\"primaryResolution\":\"" + args[0] + "\",\"secondaryResolution\":\"" + args[1] + "\",\"cameraFrameRate\":\"" + args[2] + "\",\"isAutoMulticast\":\"" + args[3] + "\",\"multicastAddressBlock\":\"" + args[4] + "\"}";
                            break;
                        }
                    case ("network_services_update"):
                        {
                            url = "/api/v0/cores/self/config/network/services"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("reflect_unregistration"):
                        {
                            url = "/api/v0/cores/self/pairing"; method_type = "DELETE";
                            break;
                        }
                    case ("reflect_registration"):
                        {
                            url = "/api/v0/cores/self/pairing"; method_type = "POST";
                            payload = "{\"hostname\":\"" + args[0] + "\",\"accessLevelName\":\"" + args[1] + "\"}";
                            break;
                        }
                    case ("delete_cameras_privacy_image"):
                        {
                            url = "/api/v0/systems/1/cameras/settings/privacyImage"; method_type = "DELETE";
                            break;
                        }
                    case ("delete_cameras_exiting_privacy_image"):
                        {
                            url = "/api/v0/systems/1/cameras/settings/exitingPrivacyImage"; method_type = "DELETE";
                            break;
                        }
                    case ("delete_cameras_offline_image"):
                        {
                            url = "/api/v0/systems/1/cameras/settings/offlineImage"; method_type = "DELETE";
                            break;
                        }

                    case ("create_custom_role"):
                        {
                            url = "/api/v0/cores/self/roles?include=actionGroups&include=usersCount&meta=permissions"; method_type = "POST";
                            payload = userpayload;
                            break;
                        }
                    case ("edit_custom_role"):
                        {
                            url = "/api/v0/cores/self/roles/" + args[0] + "?include=actionGroups&include=usersCount&meta=permissions"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("update_sofphones_settings"):
                        {
                            url = "/api/v0/systems/1/telephony"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("login"):
                        {
                            url = "/api/v0/logon"; method_type = "POST";
                            payload = "{\"username\":\"" + args[0] + "\",\"password\":\"" + args[1] + "\"}";
                            break;
                        }

                    case ("audio_files_playlist_delete"):
                        {
                            url = "/api/v0/cores/self/media_playlists/" + args[0] + ""; method_type = "DELETE";
                            break;
                        }
                    case ("audio_files_delete_file_from_playlist"):
                        {
                            url = "/api/v0/cores/self/media_playlists/" + args[0] + "/media/" + args[1] + ""; method_type = "DELETE";
                            break;
                        }
                    case ("update_password"):
                        {
                            url = "/api/v0/cores/self/users/self/password"; method_type = "PUT";
                            payload = "{\"password\":\"" + args[0] + "\",\"passwordConfirm\":\"" + args[1] + "\"}";
                            break;
                        }
                    case ("update_profile"):
                        {
                            url = "/api/v0/cores/self/users/self/profile"; method_type = "PUT";
                            payload = "{\"username\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("enable_access_control"):
                        {
                            url = "/api/v0/cores/self/access_mode"; method_type = "PUT";
                            payload = "{\"accessMode\":\"protected\",\"rootUser\":{\"username\":\"" + args[0] + "\",\"password\":\"" + args[1] + "\",\"passwordConfirm\":\"" + args[1] + "\"},\"removeUsers\": \"false\"}";
                            break;
                        }
                    case ("do_reflect_test_connection"):
                        {
                            url = "/api/v0/cores/self/pairing/debug"; method_type = "POST";
                            payload = "{\"hostname\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("audio_files_playlist_rename"):
                        {
                            url = "/api/v0/cores/self/media_playlists/" + args[0] + ""; method_type = "PUT";
                            payload = "{\"name\":\"" + args[1] + "\"}";
                            break;
                        }
                    case ("create_contact_book"):
                        {
                            url = "/api/v0/systems/1/contact_books" + ""; method_type = "POST";
                            payload = "{\"type\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("logout"):
                        {
                            url = "/api/v0/logon"; method_type = "DELETE";
                            break;
                        }
                    case ("audio_files_upload_file"):
                        {
                            ///payload:localfilepath;filemimetypr
                            ///destinationpath
                            // Testdata
                            //List<string> args1 = new List<string>();
                            //args1.Add("Audio/nirmala1");
                            //args1.Add(@"C:\Users\guru\Downloads\1000.wav");
                            //args1.Add("audio/wave");
                            url = "/api/v0/cores/self/media/" + args[2] + ""; method_type = "POST";
                            payload = args[0] + ";" + args[1];
                            fileContentName = "media";
                            break;
                        }
                    case ("snmp_update"):
                        {
                            url = "/api/v0/systems/1/snmp"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("date_time_update"):
                        {
                            url = "/api/v0/cores/self/config/time"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("ntp_update"):
                        {
                            url = "/api/v0/cores/self/config/ntp"; method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("csr_create"):
                        {
                            url = "/api/v0/cores/self/certificates/csr"; method_type = "POST";
                            payload = userpayload;
                            break;
                        }
                    case ("csr_upload_template"):
                        {
                            url = "/api/v0/cores/self/certificates/csr"; method_type = "PUT";
                            payload = args[0] + ";" + "application/octet-stream";
                            fileContentName = "csr";
                            break;
                        }
                    case ("csr_reset"):
                        {
                            url = "/api/v0/cores/self/certificates/csr"; method_type = "DELETE";
                            break;
                        }
                    case ("certificate_remove"):
                        {
                            url = "/api/v0/cores/self/certificates/certificate"; method_type = "DELETE";
                            break;
                        }
                     default:
                        {
                            no_Such_Method = true;
                            break;
                        }
                }

                if (!String.IsNullOrEmpty(url)&&(!no_Such_Method))
                {
                    combine.Add("URL", "http://" + ipaddress + url);
                    combine.Add("Method_Type", method_type);
                    combine.Add("Payload", payload);
                    combine.Add("token", token);
                    combine.Add("Method_Name", methodname);
                    if (methodname == "audio_files_upload_file" || methodname == "csr_upload_template")
                    {
                        combine.Add("name", fileContentName);
                        outputdata = mulipartdata_upload(combine);
                    }
                    else
                        outputdata = HttpGetactual_json(combine);
                }
                else {
                    if (no_Such_Method)
                        outputdata = new Tuple<bool, string>(false, methodname+"-No such method available");
                }
                return outputdata;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                //   return new string[] { "False", methodname + "- Exception during conversion" + ex.ToString(), string.Empty };
                //return new Tuple<bool, string>(false, "Error in payload and arguments processing-" + string.Join(",",args) + Environment.NewLine + ex.ToString());
                return new Tuple<bool, string>(false, "Wrong no of user input arguments-(" + string.Join(",", args) + ")");
            }
        }

        public Tuple<bool,string> QRCM_HTTPVerification(string methodname, string token,string ipaddress, List<string> args)
        {
            bool no_Such_Method = false;
            string url = string.Empty;
            string method_type = string.Empty;
            //  string outputdata = string.Empty;
            Tuple<bool, string> outputdata = new Tuple<bool, string>(false,string.Empty);
            Dictionary<string, string> combine = new Dictionary<string, string>();
            try
            {
                switch (methodname)
                {

                    case ("audio_files_playlist_details"):
                        {
                            url = "/api/v0/cores/self/media_playlists/" + args[0] + ""; method_type = "GET";
                            break;
                        }
                    case ("reflect_test_connection_latency"):
                        {
                            url = "/api/v0/cores/self/pairing/debug/latency?hostname=" + args[0] + ""; method_type = "GET";
                            break;
                        }
                    case ("details_802_1x_specified_interface"):
                        {
                            url = "/api/v0/cores/self/802_1x?meta=permissions&interface=" + args[0] + ""; method_type = "GET";
                            break;
                        }

                    case ("systems_details"):
                        {
                            url = "/api/v0/systems?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("core_details"):
                        {
                            url = "/api/v0/cores/self?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("inventory_details"):
                        {
                            url = "/api/v0/systems/1/items?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("events_details"):
                        {
                            url = "/api/v0/systems/1/events?page=1&pageSize=100&meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("date_time_details"):
                        {
                            url = "/api/v0/cores/self/config/time?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("ntp_details"):
                        {
                            url = "/api/v0/cores/self/config/ntp"; method_type = "GET";
                            break;
                        }
                    case ("network_settings_details"):
                        {
                            url = "/api/v0/cores/self/config/network?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("id_mode_details"):
                        {
                            url = "/api/v0/cores/self/config/id_mode?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("network_services_details"):
                        {
                            url = "/api/v0/cores/self/config/network/services?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("certificates_details"):
                        {
                            url = "/api/v0/cores/self/certificates/certificate?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("details_802_1x_interfaces"):
                        {
                            url = "/api/v0/cores/self/802_1x/interfaces"; method_type = "GET";
                            break;
                        }

                    case ("snmp_details"):
                        {
                            url = "/api/v0/systems/1/snmp?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("licensing_code_details"):
                        {
                            url = "/api/v0/cores/self/licensing/code?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("licensing_connectivity_details"):
                        {
                            url = "/api/v0/cores/self/licensing/connectivity?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("licensing_licenses_details"):
                        {
                            url = "/api/v0/cores/self/licensing/licenses?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("user_profile_details"):
                        {
                            url = "/api/v0/cores/self/users/self?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("users_details"):
                        {
                            url = "/api/v0/cores/self/users?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("user_roles_details"):
                        {
                            url = "/api/v0/cores/self/roles?include=usersCount&include=actionGroups&meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("audio_files_details"):
                        {
                            url = "/api/v0/cores/self/media?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("audio_files_playlists_details"):
                        {
                            url = "/api/v0/cores/self/media_playlists?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("utilities_details"):
                        {
                            url = "/api/v0/cores/self/debug/network/df?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("camera_firmware_details"):
                        {
                            url = "/api/v0/systems/1/cameras/firmware?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("video_endpoints_details"):
                        {
                            url = "/api/v0/systems/1/videos/settings/idle_screen/devices?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("video_endpoints_edids_details"):
                        {
                            url = "/api/v0/cores/self/edids?"; method_type = "GET";
                            break;
                        }
                    case ("softphones_details"):
                        {
                            url = "/api/v0/systems/1/telephony?meta=permissions"; method_type = "GET";
                            break;
                        }

                    case ("contacts_details"):
                        {
                            url = "/api/v0/systems/1/contact_books?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("uci_details"):
                        {
                            url = "/api/v0/systems/1/ucis?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("uci_pins"):
                        {
                            url = "/api/v0/systems/1/ucis/pins"; method_type = "GET";
                            break;
                        }
                    case ("reflect_registration_status"):
                        {
                            url = "/api/v0/cores/self/pairing"; method_type = "GET";
                            break;
                        }
                    case ("dynamic_pairing_details"):
                        {
                            url = "/api/v0/systems/1/device_pairing?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("camera_settings_details"):
                        {
                            url = "/api/v0/systems/1/cameras/settings?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("video_endpoints_idle_screen_details"):
                        {
                            url = "/api/v0/systems/1/videos/settings/idle_screen?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("video_endpoints_custom_graphics_details"):
                        {
                            url = "/api/v0/systems/1/videos/settings/custom_graphics?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("items"):
                        {
                            url = "/api/v0/systems/1/items?"; method_type = "GET";
                            break;
                        }
                    case ("revisions"):
                        {
                            url = "/api/v0/systems/1/revisions"; method_type = "GET";
                            break;
                        }
                    case ("is_access_control_enabled"):
                        {
                            url = "/api/v0/cores/self/access_mode?meta=permissions"; method_type = "GET";
                            break;
                        }
                    case ("audio_files_storage"):
                        {
                            url = "/api/v0/cores/self/media?meta=storage"; method_type = "GET";
                            break;
                        }
                    case ("audio_files_directory_content"):
                        {
                            url = "/api/v0/cores/self/media/"+args[0]+""; method_type = "GET";
                            break;
                        }
                    case ("csr_details"):
                        {
                            url = "/api/v0/cores/self/certificates/csr"; method_type = "GET";
                            break;
                        }
                    case ("public_key_details"):
                        {
                            url = "/api/v0/cores/self/certificates/pub_key"; method_type = "GET";
                            break;
                        }
                    case ("reflect_test_connection_status"):
                        {
                            url = "/api/v0/cores/self/pairing/debug?"; method_type = "GET";
                            break;
                        }
                    case ("multicast_details"):
                        {
                            url = "/api/v0/cores/self/multicast?meta=permissions"; method_type = "GET";
                            break;

                        }
                    case ("video_endpoints_multicast_details"):
                        {
                            url = "/api/v0/systems/1/multicast?meta=permissions"; method_type = "GET";
                            break;
                        }
                    default:
                        {
                            no_Such_Method = true;
                            break;
                        }


                }
                if (!String.IsNullOrEmpty(url)&&(!no_Such_Method))
                {
                    combine.Add("URL", "http://"+ipaddress +url);
                    combine.Add("Method_Type", method_type);
                    combine.Add("Payload", "");
                    combine.Add("token", token);
                    combine.Add("Method_Name", methodname);
                    outputdata = HttpGetactual_json(combine);
                }
                
                else
                {
                    if (no_Such_Method)
                        outputdata = new Tuple<bool, string>(false, methodname + "-No such method available");
                }
                return outputdata;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                //   return new string[] { "False", methodname + "- Exception during conversion" + ex.ToString(), string.Empty };
                //return new Tuple<bool, string>(false, "Error in payload and arguments processing-" + string.Join(",",args)+Environment.NewLine + ex.ToString());
                return new Tuple<bool, string>(false, "Wrong no of user input arguments-(" + string.Join(",", args) + ")");
            }
        }

        public  Tuple<bool,string> HttpGetactual_json(Dictionary<string, string> strURI)
        {

            //HttpWebResponse exc_response = null;
            bool j_val = false;
            string strResponse = string.Empty;
            bool status = false;
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(strURI["URL"]);
            int code = -1;
            object error = null;
            try
            {

                req.ContentType = "application/json";
                // req.Accept = "*/*";
                //req.Accept = string.Empty;
                req.Method = strURI["Method_Type"];
                req.Timeout = 30000;
                req.ReadWriteTimeout = 30000;
                req.KeepAlive = true;
			    req.Accept = "application/json";
               // req.Method = strURI["Method_Type"];
                if (strURI["token"] != string.Empty)
                    req.Headers["Authorization"] = "Bearer " + strURI["token"];
                
                if ((req.Method.ToString().ToUpper() == "POST") || (req.Method.ToString().ToUpper() == "PUT")|| (req.Method.ToString().ToUpper() == "DELETE")&& (req.Method.ToString().ToUpper() != "GET"))
                {
                    Byte[] retBytes = System.Text.Encoding.ASCII.GetBytes(strURI["Payload"]);
                    req.ContentLength = retBytes.Length;

                    using (System.IO.Stream outStream = req.GetRequestStream())
                    {
                        outStream.Write(retBytes, 0, retBytes.Length);
                        outStream.Close();
                    }
                }
                using (HttpWebResponse json_response = (HttpWebResponse)req.GetResponse())
                {
                    code = (int)(json_response.StatusCode);

                    //exc_response = json_response;
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(json_response.GetResponseStream()))
                    {

                        strResponse = sr.ReadToEnd().Trim();
                        j_val = test(strResponse, strURI["Method_Name"]);
                        status = true;
                        //dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strResponse);
                        // strResponse = array.Tostring();

                    }


                }

                req.Abort();
                if ((strResponse == string.Empty)||j_val)

                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + ",\""+strResponse+"\"],\"error\":\"" + error + "\"}");
                else
                //return "{\"QATdata\": [" + code + ",\"" + strResponse + "\"], \"error\": \"" + null + "\"}";
                return new Tuple<bool,string>(status,"{\"QATdata\":["+code+","+strResponse+"],\"error\":\""+error+"\"}");
               // return strResponse;
            }
            catch (WebException ex)
            {
                
                if (ex.Response != null)
                {
                    HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                    code = (int)(errorResponse.StatusCode);
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                        {                           
                            strResponse = sr.ReadToEnd().Trim();
                        j_val = test(strResponse,string.Empty);
                        status = true;

                    }
                 }
                else
                {
                    strResponse = ex.Message.ToString();
                    error = "Req_error";
                    status = false;
                    if (strResponse.Contains("Unable to connect to the remote server"))
                    { req.Abort(); return new Tuple<bool, string>(status, "Core not available in network");}
                }
                req.Abort();

               if(!j_val)
                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + "," + strResponse + " ],\"error\":\"" +error+"\"}");
              else
                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + ",\"" + strResponse + "\" ],\"error\":\"" + error + "\"}");

            }
            catch (Exception ex)
            {
                req.Abort();
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                return new Tuple<bool, string>(false, "{\"QATdata\":[" + code + "," + ex.Message.ToString() + "],\"error\":\"Req_error\"}");

            }
        }

        public Tuple<bool, string> mulipartdata_upload(Dictionary<string,string> strURI)
        {
            bool j_val = false;
            string strResponse = string.Empty;            
            int code = -1;
            object error = null;
            bool status = false;
            System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(strURI["URL"]);
            try
            {
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                string contentType = "multipart/form-data; boundary=" + formDataBoundary;
                request.Method = strURI["Method_Type"];
                request.Accept = "application/json";
                request.ContentType = contentType;
                Encoding encoding = Encoding.UTF8;
                if (strURI["token"] != string.Empty)
                    request.Headers["Authorization"] = "Bearer " + strURI["token"];

                string[] filedata=strURI["Payload"].Split(';');
                using (MemoryStream poststream = new System.IO.MemoryStream())
                {
                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    //dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(strURI["Payload"]);

                    // foreach (var dd in array)
                    //{
                    //var dta = strURI["Payload"];
                    //dta[""];
                        string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", formDataBoundary, strURI["name"], Path.GetFileName(filedata[0]), filedata[1]);
                        poststream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        using (FileStream inStream = new FileStream(Path.GetFullPath(filedata[0]), FileMode.Open, FileAccess.Read))
                        {
                            const int inBufferSize = 32768;//32kb
                            while (inStream.Position < inStream.Length)
                            {
                                byte[] chunkData = new byte[inBufferSize];
                                int chunkDataRead = inStream.Read(chunkData, 0, inBufferSize);
                                poststream.Write(chunkData, 0, chunkDataRead);
                            }
                        }
                    //}


                    poststream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    string footer = "\r\n--" + formDataBoundary + "--\r\n";

                    poststream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                    // Dump the Stream into a byte[]-->formData 
                    poststream.Position = 0;
                    byte[] formData = new byte[poststream.Length];
                    poststream.Read(formData, 0, formData.Length);
                    poststream.Close();

                    request.ContentLength = formData.Length;

                    // Send the form data to the request.
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(formData, 0, formData.Length);
                        requestStream.Close();
                    }

                    
                }
                using (HttpWebResponse json_response = (HttpWebResponse)request.GetResponse())
                {
                    code = (int)(json_response.StatusCode);

                  
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(json_response.GetResponseStream()))
                    {

                        strResponse = sr.ReadToEnd().Trim();
                        j_val = test(strResponse,string.Empty);
                        status = true;



                    }


                }

                request.Abort();
                if ((strResponse == string.Empty) || j_val)

                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + ",\"" + strResponse + "\"],\"error\":\"" + error + "\"}");
                else
                    //return "{\"QATdata\": [" + code + ",\"" + strResponse + "\"], \"error\": \"" + null + "\"}";
                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + "," + strResponse + "],\"error\":\"" + error + "\"}");
            }
            catch (WebException ex)
            {
               
                if (ex.Response != null)
                {
                    HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                    code = (int)(errorResponse.StatusCode);
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                    {

                        strResponse = sr.ReadToEnd().Trim();
                        j_val = test(strResponse,string.Empty);
                        status = true;

                    }
                    

                }
                else
                {
                    strResponse = ex.Message.ToString();
                    error = "Req_error";
                    status = false;

                    if (strResponse.Contains("Unable to connect to the remote server"))
                    { request.Abort(); return new Tuple<bool, string>(status, "Core not available in network"); }
                }
                request.Abort();

                if (!j_val)
                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + "," + strResponse + " ],\"error\":\"" + error + "\"}");
                else
                    return new Tuple<bool, string>(status, "{\"QATdata\":[" + code + ",\"" + strResponse + "\" ],\"error\":\"" + error + "\"}");

            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                request.Abort();
                return new Tuple<bool, string>(false, "{\"QATdata\":[" + code + "," + ex.Message.ToString() + "],\"error\":\"Req_error\"}");

            }

        }
  
        public bool test(string data,string M_name)
        {
            bool validjson = false;
            try
            {
                dynamic array = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(data);
                if (M_name == "login")
                {
                    //dynamic array1 = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(data);
                    if (array != null && array.Count>0 )
                    {
                        foreach (var item in array)
                        {
                            if (item.Key == "token")
                            {
                                //string dgt = item.Value;
                                QRCMLogonToken = item.Value;
                            }
                        }

                    }
                }

            }
           
            catch (Exception ex)
            {
               
                validjson = true;
            }
            return validjson;


        }
    }

    public class QREM_API : QRCM_API
    {
        //QRCM_API qrcmobj = new QRCM_API();

        public Tuple<bool, string> QREM_HTTPAction(string methodname, Dictionary<string, string> idlist, List<string> args, string userpayload)
        {
            string url = string.Empty;
            string method_type = string.Empty;
            string payload = string.Empty;
            string fileContentName = string.Empty;
            Tuple<bool, string> outputdata = new Tuple<bool, string>(false, string.Empty);
            Dictionary<string, string> combine = new Dictionary<string, string>();
            bool no_Such_Method = false;
            if (userpayload == "None")
                userpayload = string.Empty;

            try
            {
                switch (methodname)
                {
                    case ("enable_access_control"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/access_mode";
                            method_type = "PUT";
                            payload = "{\"accessMode\":\"protected\",\"rootUser\":{\"username\":\"" + args[0] + "\",\"password\":\"" + args[1] + "\",\"passwordConfirm\":\"" + args[1] + "\"},\"removeUsers\": \"false\"}";
                            break;
                        }
                    case ("disable_access_control"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/access_mode";
                            method_type = "PUT";
                            payload = "{\"accessMode\": \"open\", \"removeUsers\": \"true\"}";
                            break;
                        }
                    case ("clear_events"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/events";
                            method_type = "DELETE";
                            break;
                        }
                    case ("create_new_user"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/users";
                            method_type = "POST";
                            payload = "{\"role\": \"" + args[0] + "\", \"username\": \"" + args[1] + "\", \"password\": \"" + args[2] + "\",\"passwordConfirm\": \"" + args[3] + "\"}";
                            break;
                        }
                    case ("update_user_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/users/" + args[0];
                            method_type = "PUT";
                            payload = "{\"id\": \"" + args[0] + "\",\"role\": \"" + args[2] + "\", \"username\": \"" + args[1] + "\"}";
                            break;
                        }
                    case ("update_user_password"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/users/" + args[0] + "/password";
                            method_type = "PUT";
                            payload = "{\"password\": \"" + args[1] + "\",\"passwordConfirm\": \"" + args[2] + "\"}";
                            break;
                        }

                    case ("delete_user"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/users/" + args[0];
                            method_type = "DELETE";
                            break;
                        }
                    case ("delete_custom_role"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/roles/" + args[0];
                            method_type = "DELETE";
                            break;
                        }
                    case ("delete_cameras_privacy_image"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/cameras/settings/privacyImage";
                            method_type = "DELETE";
                            break;
                        }
                    case ("delete_cameras_exiting_privacy_image"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/cameras/settings/exitingPrivacyImage";
                            method_type = "DELETE";
                            break;
                        }
                    case ("delete_cameras_offline_image"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/cameras/settings/offlineImage";
                            method_type = "DELETE";
                            break;
                        }
                    case ("create_contact_book"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/contact_books";
                            method_type = "POST";
                            payload = "{\"type\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("audio_files_playlist_rename"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists/" + args[0];
                            method_type = "PUT";
                            payload = "{\"name\":\"" + args[1] + "\"}";
                            break;
                        }
                    case ("audio_files_playlist_delete"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists/" + args[0];
                            method_type = "DELETE";
                            break;
                        }
                    case ("audio_files_delete_file_from_playlist"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists/" + args[0] + "/media/" + args[1];
                            method_type = "DELETE";
                            break;
                        }
                    case ("audio_files_create_directory"):
                        {
                            if (args[0].Contains('/'))
                            {
                                string argval = args[0];
                                if (argval.EndsWith("/"))
                                {
                                    argval = argval.Remove(argval.Length - 1);
                                }

                                string[] url_construct = argval.Split('/');
                                payload = "{\"name\": \"" + url_construct.Last() + "\"}";
                                url = "/api/v0/cores/" + idlist["core_id"] + "/media/" + string.Join("/", url_construct.Take(url_construct.Length - 1));
                                method_type = "POST";
                            }
                            else
                            {
                                url = "/api/v0/cores/" + idlist["core_id"] + "/media";
                                method_type = "POST";
                                payload = "{\"name\": \"" + args[0] + "\"}";
                            }
                            break;
                        }
                    case ("audio_files_rename_file_directory"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media";
                            method_type = "PUT";
                            payload = "[{\"" + args[0] + "\": { \"path\": \"" + args[1] + "\"}}]";
                            break;
                        }
                    case ("audio_files_delete_file_directory"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media";
                            method_type = "DELETE";
                            payload = "[\"" + args[0] + "\"]";
                            break;
                        }
                    case ("audio_files_create_playlist"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists";
                            method_type = "POST";
                            payload = "{\"name\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("audio_files_add_file_to_playlist"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists/" + args[0] + "/media";
                            method_type = "POST";
                            payload = "[{\"path\":\"" + args[1] + "\"}]";
                            break;
                        }
                    case ("reboot"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/reboot";
                            method_type = "PUT";
                            break;
                        }
                    case ("update_contact_book"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/contact_books/" + args[0];
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("delete_contact_book"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/contact_books/" + args[0];
                            method_type = "DELETE";
                            break;
                        }
                    case ("update_camera_settings"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/cameras/settings";
                            method_type = "PUT";
                            payload = "{\"primaryResolution\":\"" + args[0] + "\",\"secondaryResolution\":\"" + args[1] + "\",\"cameraFrameRate\":\"" + args[2] + "\",\"isAutoMulticast\":\"" + args[3] + "\",\"multicastAddressBlock\":\"" + args[4] + "\"}";
                            break;
                        }
                    case ("create_custom_role"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/roles?include=actionGroups&include=usersCount";
                            method_type = "POST";
                            payload = userpayload;
                            break;
                        }
                    case ("edit_custom_role"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/roles/" + args[0] + "?include=actionGroups&include=usersCount";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("update_sofphones_settings"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/telephony";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("network_services_update"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/network/services";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("rename_system"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"];
                            method_type = "PUT";
                            payload = "{\"name\":\"" + args[0] + "\"}";
                            break;
                        }
                    case ("add_uci_pin"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/ucis/pins";
                            method_type = "POST";
                            payload = "[{\"name\":\"" + args[0] + "\",\"pin\":\"" + args[1] + "\"}]";
                            break;
                        }
                    case ("add_pin_to_uci"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/ucis";
                            method_type = "PUT";
                            payload = "{\"fileName\":\"" + args[0] + "\",\"pinIds\":[\"" + args[1] + "\"]}";
                            break;
                        }
                    case ("modify_uci_pin"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/ucis/pins";
                            method_type = "PUT";
                            payload = "[{\"id\":\"" + args[0] + "\",\"name\":\"" + args[1] + "\",\"pin\":\"" + args[2] + "\"}]";
                            break;
                        }
                    case ("delete_uci_pin"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/ucis/pins?id=" + args[0];
                            method_type = "DELETE";
                            break;
                        }
                    case ("network_settings_update"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/network";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("audio_files_upload_file"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media/" + args[2];
                            method_type = "POST";
                            payload = args[0] + ";" + args[1];
                            fileContentName = "media";
                            break;
                        }
                    case ("csr_upload_template"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/csr";
                            method_type = "PUT";
                            payload = args[0] + ";" + "application/octet-stream";
                            fileContentName = "csr";
                            break;
                        }
                    case ("snmp_update"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/snmp";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("date_time_update"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/time";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("ntp_update"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/ntp";
                            method_type = "PUT";
                            payload = userpayload;
                            break;
                        }
                    case ("csr_create"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/csr";
                            method_type = "POST";
                            payload = userpayload;
                            break;
                        }
                    case ("csr_reset"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/csr";
                            method_type = "DELETE";
                            break;
                        }
                    case ("certificate_remove"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/certificate";
                            method_type = "DELETE";
                            break;
                        }
                    default:
                        {
                            no_Such_Method = true;
                            break;
                        }
                }

                if (!String.IsNullOrEmpty(url) && (!no_Such_Method))
                {
                    combine.Add("URL", "https://" + Properties.Settings.Default.QREMreflectLink + url);
                    combine.Add("Method_Type", method_type);
                    combine.Add("Payload", payload);
                    combine.Add("token", DeviceDiscovery.QREM_Token);
                    combine.Add("Method_Name", methodname);
                    //combine.Add("CoreName", idlist["CoreName"]);
                    //combine.Add("FilepathToSave", idlist["FilepathToSave"]);

                    if (methodname == "audio_files_upload_file" || methodname == "csr_upload_template")
                    {
                        combine.Add("name", fileContentName);
                        outputdata = mulipartdata_upload(combine);
                    }
                    else
                    {
                        outputdata = HttpGetactual_json(combine);
                    }
                }
                else
                {
                    if (no_Such_Method)
                        outputdata = new Tuple<bool, string>(false, methodname + "-No such method available");
                }
                return outputdata;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                return new Tuple<bool, string>(false, "Wrong no of user input arguments-(" + string.Join(",", args) + ")");
            }
        }

        public Tuple<bool, string> QREM_HTTPVerification(string methodname, Dictionary<string, string> idlist, List<string> args)
        {
            bool no_Such_Method = false;
            string url = string.Empty;
            Tuple<bool, string> outputdata = new Tuple<bool, string>(false, string.Empty);
            Dictionary<string, string> combine = new Dictionary<string, string>();
            try
            {
                switch (methodname)
                {
                    case ("network_settings_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/network";
                            break;
                        }
                    //case ("site_cores_details"):
                    //    {
                    //        url = "/api/v0/sites/" + idlist["site_id"] + "/cores";
                    //        break;
                    //    }
                    case ("inventory_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/items";
                            break;
                        }
                    case ("items"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/items";
                            break;
                        }
                    case ("events_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/events?pageSize=100&page=1";
                            break;
                        }
                    case ("date_time_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/time";
                            break;
                        }
                    case ("ntp_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/ntp";
                            break;
                        }
                    case ("licensing_licenses_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/licensing/licenses";
                            break;
                        }
                    case ("licensing_connectivity_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/licensing/connectivity";
                            break;
                        }
                    case ("licensing_code_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/licensing/code";
                            break;
                        }
                    case ("network_services_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/network/services";
                            break;
                        }
                    case ("details_802_1x_interfaces"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/802_1x/interfaces";
                            break;
                        }
                    case ("details_802_1x_specified_interface"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/802_1x?interface=" + args[0];
                            break;
                        }
                    case ("certificates_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/certificate";
                            break;
                        }
                    case ("users_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/users";
                            break;
                        }
                    case ("user_roles_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/roles?include=usersCount&include=actionGroups&meta=permissions";
                            break;
                        }
                    case ("audio_files_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media";
                            break;
                        }
                    case ("audio_files_playlists_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists";
                            break;
                        }
                    case ("audio_files_storage"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media?meta=storage";
                            break;
                        }
                    case ("audio_files_directory_content"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media/" + args[0];
                            break;
                        }
                    case ("audio_files_playlist_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/media_playlists/" + args[0];
                            break;
                        }
                    case ("uci_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/ucis";
                            break;
                        }
                    case ("softphones_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/telephony";
                            break;
                        }
                    case ("contacts_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/contact_books";
                            break;
                        }
                    case ("camera_settings_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/cameras/settings";
                            break;
                        }
                    case ("camera_firmware_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/cameras/firmware";
                            break;
                        }
                    case ("video_endpoints_idle_screen_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/videos/settings/idle_screen";
                            break;
                        }
                    case ("video_endpoints_custom_graphics_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/videos/settings/custom_graphics";
                            break;
                        }
                    case ("multicast_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/multicast";
                            break;
                        }
                    case ("video_endpoints_edids_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/edids";
                            break;
                        }
                    case ("video_endpoints_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/videos/settings/idle_screen/devices";
                            break;
                        }
                    case ("dynamic_pairing_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/device_pairing";
                            break;
                        }
                    case ("id_mode_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/config/id_mode";
                            break;
                        }
                    case ("snmp_details"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/snmp";
                            break;
                        }
                    case ("public_key_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/pub_key";
                            break;
                        }
                    case ("csr_details"):
                        {
                            url = "/api/v0/cores/" + idlist["core_id"] + "/certificates/csr";
                            break;
                        }
                    case ("systems_details_remote"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"];
                            break;
                        }
                    case ("uci_pins"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/ucis/pins";
                            break;
                        }
                    case ("revisions"):
                        {
                            url = "/api/v0/systems/" + idlist["system_id"] + "/revisions";
                            break;
                        }
                    default:
                        {
                            no_Such_Method = true;
                            break;
                        }
                }

                if (!String.IsNullOrEmpty(url) && (!no_Such_Method))
                {
                    combine.Add("URL", "https://" + Properties.Settings.Default.QREMreflectLink + url);
                    combine.Add("Method_Type", "GET");
                    combine.Add("token", DeviceDiscovery.QREM_Token);
                    combine.Add("Method_Name", methodname);

                    outputdata = HttpGetactual_json(combine);
                }
                else
                {
                    if (no_Such_Method)
                        outputdata = new Tuple<bool, string>(false, methodname + "-No such method available");
                }
                return outputdata;
            }
            catch (Exception ex)
            {
                DeviceDiscovery.WriteToLogFile("Exception in " + ex.TargetSite.Name + ". Message:" + ex.Message);
                return new Tuple<bool, string>(false, "Wrong no of user input arguments-(" + string.Join(",", args) + ")");
            }
        }
    }
}

    

   


