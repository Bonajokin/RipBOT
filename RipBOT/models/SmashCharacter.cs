using Newtonsoft.Json.Linq;

namespace RipBOT.models
{
    class SmashCharacter
    {
        /// <summary>
        ///     Constructor to throw JSON string into model
        /// </summary>
        public SmashCharacter(string json)
        {
            JObject obj = JObject.Parse(json);
            Name = (string)obj["name"];
            ImageURL = (string)obj["thumbnailUrl"];
            CharId = (string)obj["id"];
        }

        // Model Attributes
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string CharId { get; set; }
    }
}
