using MongoDB.Bson.Serialization.Attributes;

public class UserModel : BaseModel
{
    [BsonElement("name")]
    public string Name;
}