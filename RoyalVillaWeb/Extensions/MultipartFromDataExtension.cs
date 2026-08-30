namespace RoyalVillaWeb.Extensions
{
    public static class MultipartFromDataExtension
    {
        public static MultipartFormDataContent ToMultipartFormData(this object obj)
        {
            var formData = new MultipartFormDataContent();
            var proparties = obj.GetType().GetProperties();

            foreach (var property in proparties)
            {
                var value = property.GetValue(obj);

                if (value == null)
                {
                    continue;
                }

                var propartyName = property.Name;

                if (value is IFormFile file && file.Length > 0)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    formData.Add(streamContent, propartyName, file.FileName);
                }
                else if (value is not IFormFile)
                {
                    formData.Add(new StringContent(value.ToString()!), propartyName);
                }
            }

            return formData;
        }
    }
}
