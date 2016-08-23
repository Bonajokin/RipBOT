using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RipBOT.models
{
    class CharacterMovements
    {
        /// <summary>
        ///     Constructor to throw JSON string into model
        /// </summary>
        public CharacterMovements(string json)
        {
            try
            {
                JArray arr = JArray.Parse(json);
                foreach (JObject obj in arr.Children<JObject>())
                {
                    CharacterMovement movement = new CharacterMovement()
                    {
                        Name = (string)obj["name"],
                        Value = (string)obj["value"]
                    };
                    movements.Add(movement);
                }
            }
            catch (Exception ex)
            {
                movements = new List<CharacterMovement>();
            }
        }

        // A single character's movement details
        public List<CharacterMovement> movements = new List<CharacterMovement>();
    }

    class CharacterMovement
    {
        // Model Attributes
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
