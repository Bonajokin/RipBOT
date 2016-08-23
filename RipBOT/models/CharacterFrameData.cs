using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RipBOT.models
{
    class CharacterFrameData
    {
        /// <summary>
        ///     Constructor to throw JSON string into model
        /// </summary>
        public CharacterFrameData(string json)
        {
            try
            {
                JArray arr = JArray.Parse(json);
                foreach (JObject obj in arr.Children<JObject>())
                {
                    CharacterMove move = new CharacterMove()
                    {
                        Name = (string)obj["name"],
                        HitboxActive = (string)obj["hitboxActive"],
                        FirstActionableFrame = (string)obj["firstActionableFrame"],
                        BaseDamage = (string)obj["baseDamage"],
                        LandingLag = (string)obj["landingLag"],
                        AutoCancel = (string)obj["autoCancel"],
                        Type = (string)obj["type"]
                    };
                    moves.Add(move);
                }   
            }
            catch (Exception ex)
            {
                moves = new List<CharacterMove>();
            }
        }

        // A single character's moves
        public List<CharacterMove> moves = new List<CharacterMove>();
    }

    class CharacterMove
    {
        // Model Attributes
        public string Name { get; set; }
        public string HitboxActive { get; set; }
        public string FirstActionableFrame { get; set; }
        public string BaseDamage { get; set; }
        public string LandingLag { get; set; }
        public string AutoCancel { get; set; }
        public string Type { get; set; }
    }
}
