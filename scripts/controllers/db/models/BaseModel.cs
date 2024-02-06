using MongoDB.Bson.Serialization.Attributes;

public partial class BaseModel
{
    [BsonElement("id")]
    public string Id;
}