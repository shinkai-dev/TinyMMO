using Godot;
using MongoDB.Bson.Serialization.Attributes;

public partial class UserModel : BaseModel
{
    [BsonElement("name")]
    public string Name;
    [BsonElement("color")]
    public ColorModel Color;
}