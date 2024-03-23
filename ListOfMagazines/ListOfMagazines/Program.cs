// See https://a ka.ms/new-console-template for more information
using System.Text;
using System.Text.Json;


#region Getting the Token
Task<string> task = GetToken();
string tokenString = await task;
//Console.WriteLine("Getting Token Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
#endregion

#region Getting the Categories of Magazines
CategoryObject Categories = new CategoryObject();
Task<CategoryObject> CategoriesTask = GetCategories(tokenString);
Categories = await CategoriesTask;
//Console.WriteLine("Getting Categories Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
#endregion

#region Getting each Category magazine subscriber details.

Dictionary<string, List<int>> GroupMagazineByCateg = new Dictionary<string, List<int>>();
if (Categories.data != null && Categories.data.Count > 0)
{
    MagazineObject magazine = null;

    List<Task<MagazineObject>> tasks = new List<Task<MagazineObject>>();
    foreach (string category in Categories.data)
    {
        var MagazineTask = GetMagazinedtl(tokenString, category);
        tasks.Add(MagazineTask);
    }

    await Task.WhenAll(tasks);

    foreach (var magazineetask in tasks)
    {
        if (magazineetask.IsCompletedSuccessfully)
        {
            var ListofMagazineIdforCategory = from list in magazineetask.Result.data
                                              select list.id;
            var Category = magazineetask.Result.data[0].category;
            GroupMagazineByCateg.Add(Category.ToString(), ListofMagazineIdforCategory.ToList());
            //Console.WriteLine("Magazine Processing Token Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
        }
    }
}
#endregion

#region Identify all subscribers that are subscribed to at least one magazine in each category.
string strIDs = string.Empty;
List<string> lstSubscriberIDs = new List<string>();
SubscribersObject subscribers = new SubscribersObject();
Task<SubscribersObject> SubscriberTask = GetSubscriberdtl(tokenString);
subscribers = await SubscriberTask;
if (subscribers != null)
{


    Dictionary<string, bool> category = new Dictionary<string, bool>();
    foreach (SubscriberSubObject eachsubscriber in subscribers.data)
    {
        category.Clear();
        foreach (var magazineID in eachsubscriber.magazineIds) //2,3
        {
            foreach (var kvp in GroupMagazineByCateg)//{1,2,3}
            {
                if (kvp.Value.Contains(magazineID))
                {
                    if (!category.Keys.Contains(kvp.Key))
                    {
                        category.Add(kvp.Key, true);
                    }

                }

            }
            if (category.Count == GroupMagazineByCateg.Count)
            {
                lstSubscriberIDs.Add(eachsubscriber.id);
                category.Clear();
            }

        }
    }
    //Console.WriteLine("Finding result  Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
}

Task<string> Posttask = PostSubscriberIDs(tokenString, lstSubscriberIDs);
string Result = await Posttask;
//Console.WriteLine("Answer final Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
//Console.Read();
#endregion

#region Calling EndPoints

#region GetToken
async Task<string> GetToken()
{
    string tokenString = string.Empty;
    using (var client = new HttpClient())
    {
        try
        {
            HttpResponseMessage responseMessage = await client.GetAsync("http://magazinestore.azurewebsites.net/api/token");
            if (responseMessage.IsSuccessStatusCode)
            {
                tokenString = await responseMessage.Content.ReadAsStringAsync();
                //Console.WriteLine("Token Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));

                JsonDocument jsonDocument = JsonDocument.Parse(tokenString);

                JsonElement root = jsonDocument.RootElement;

                if (root.TryGetProperty("token", out JsonElement tokenElement))
                {
                    tokenString = tokenElement.GetString();
                }
            }
            else
            {
                Console.WriteLine($"Invalid :" + responseMessage.StatusCode);
            }

        }
        catch (Exception e) { Console.WriteLine($"Error :{e.Message}"); }
    }
    return tokenString;
}
#endregion

#region GetCategories
async Task<CategoryObject> GetCategories(string token)
{
    CategoryObject Categories = null;
    using (var client = new HttpClient())
    {

        try
        {
            HttpResponseMessage responseMessage = await client.GetAsync("http://magazinestore.azurewebsites.net/api/categories/" + token);
            if (responseMessage.IsSuccessStatusCode)
            {
                string jsonstring = await responseMessage.Content.ReadAsStringAsync();
                Categories = JsonSerializer.Deserialize<CategoryObject>(jsonstring);
                //Console.WriteLine("Categories Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
            }
            else
            {
                Console.WriteLine($"Invalid :" + responseMessage.StatusCode);
            }
        }
        catch (Exception e) { Console.WriteLine($"Error :{e.Message}"); }
    }
    return Categories;
}

#endregion

#region GetMagazinedetail
async Task<MagazineObject> GetMagazinedtl(string token, string Category)
{
    MagazineObject magazine = null;
    using (var client = new HttpClient())
    {

        try
        {
            HttpResponseMessage responseMessage = await client.GetAsync("http://magazinestore.azurewebsites.net/api/magazines/" + token + "/" + Category);
            if (responseMessage.IsSuccessStatusCode)
            {
                string jsonstring = await responseMessage.Content.ReadAsStringAsync();
                magazine = JsonSerializer.Deserialize<MagazineObject>(jsonstring);
                //Console.WriteLine(" Magazine Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
            }
            else
            {
                Console.WriteLine($"Invalid :" + responseMessage.StatusCode);
            }
        }
        catch (Exception e) { Console.WriteLine($"Error :{e.Message}"); }
    }
    return magazine;
}
#endregion

#region GetSubscriberdetail
async Task<SubscribersObject> GetSubscriberdtl(string token)
{
    SubscribersObject Subscribers = null;
    using (var client = new HttpClient())
    {
        try
        {
            HttpResponseMessage responseMessage = await client.GetAsync("http://magazinestore.azurewebsites.net/api/subscribers/" + token);
            if (responseMessage.IsSuccessStatusCode)
            {
                string jsonstring = await responseMessage.Content.ReadAsStringAsync();
                Subscribers = JsonSerializer.Deserialize<SubscribersObject>(jsonstring);
                //Console.WriteLine("Subscriber Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
            }
            else
            {
                Console.WriteLine($"Invalid :" + responseMessage.StatusCode);
            }
        }
        catch (Exception e) { Console.WriteLine($"Error :{e.Message}"); }
    }
    return Subscribers;
}

#endregion

#region PostSubscriberIDs
async Task<string> PostSubscriberIDs(string token, List<string> SubscriberIDList)
{
    string Subscribers = string.Empty;

    using (var client = new HttpClient())
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(new { subscribers = SubscriberIDList });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json-patch+json");
            string URL = $"http://magazinestore.azurewebsites.net/api/answer/{token}";

            var response = await client.PostAsync(URL, content);

            if (response.IsSuccessStatusCode)
            {
                string jsonstring = await response.Content.ReadAsStringAsync();
                //Console.WriteLine("Answer Time :" + DateTime.Now.ToString("hh:mm:ss.fff tt"));
                Console.WriteLine("Answer Endpoint Result: \n " + jsonstring);
                Console.Read();
            }
            else
            {
                Console.WriteLine($"Invalid :" + response.StatusCode);
            }
        }
        catch (Exception e) { Console.WriteLine($"Error :{e.Message}"); }
    }
    return Subscribers;
}
#endregion

#endregion

#region Defining Objects
public class CategoryObject
{
    public List<string> data { get; set; }
    public bool success { get; set; }
    public string token { get; set; }
}
public class MagazineSubObject
{
    public int id { get; set; }
    public string name { get; set; }
    public string category { get; set; }

}
public class MagazineObject
{
    public List<MagazineSubObject> data { get; set; }
    public bool success { get; set; }
    public string token { get; set; }

}
public class SubscribersObject
{
    public List<SubscriberSubObject> data { get; set; }
    public bool success { get; set; }
    public string token { get; set; }

}
public class SubscriberSubObject
{
    public string id { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public List<int> magazineIds { get; set; }
}
#endregion

