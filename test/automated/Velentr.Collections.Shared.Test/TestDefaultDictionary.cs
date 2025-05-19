using System.Text.Json;

namespace Velentr.Collections.Test;

[TestFixture]
public class TestDefaultDictionary
{
    [Test]
    public void Test_DefaultValue()
    {
        DefaultDictionary<string, int> defaultDict = new(42);
        Assert.That(defaultDict["missingKey"], Is.EqualTo(42));
    }

    [Test]
    public void Test_DefaultValueFactory()
    {
        DefaultDictionary<string, int> defaultDict = new(() => DateTime.Now.Year);
        Assert.That(defaultDict["missingKey"], Is.EqualTo(DateTime.Now.Year));
    }

    [Test]
    public void Test_SetAndGetValues()
    {
        DefaultDictionary<string, string> defaultDict = new("default");
        defaultDict["key1"] = "value1";
        Assert.That(defaultDict["key1"], Is.EqualTo("value1"));
        Assert.That(defaultDict["missingKey"], Is.EqualTo("default"));
    }

    [Test]
    public void Test_SetDefaultValueBeforeSettingValue()
    {
        DefaultDictionary<string, int> defaultDict = new(0, true);
        defaultDict["key1"] = 10;
        Assert.That(defaultDict["key1"], Is.EqualTo(10));
        Assert.That(defaultDict["missingKey"], Is.EqualTo(0));
    }

    [Test]
    public void Test_ToDictionary()
    {
        DefaultDictionary<string, int> defaultDict = new(42);
        defaultDict["key1"] = 10;
        Dictionary<string, int> standardDict = defaultDict.ToDictionary();
        Assert.That(standardDict["key1"], Is.EqualTo(10));
        Assert.That(standardDict.ContainsKey("missingKey"), Is.False);
    }

    [Test]
    public void Test_IsDefaultValueFactorySet()
    {
        DefaultDictionary<string, int> defaultDictWithFactory = new(() => 100);
        Assert.That(defaultDictWithFactory.IsDefaultValueFactorySet, Is.True);

        DefaultDictionary<string, int> defaultDictWithoutFactory = new(42);
        Assert.That(defaultDictWithoutFactory.IsDefaultValueFactorySet, Is.False);
    }

    [Test]
    public void Test_Serialization_WithDefaultValue()
    {
        DefaultDictionary<string, int> defaultDict = new(42);
        defaultDict["key1"] = 10;

        var json = JsonSerializer.Serialize(defaultDict);
        Assert.That(json, Does.Contain("\"key1\":10"));
        Assert.That(json, Does.Contain("\"defaultValue\":42"));
    }

    [Test]
    public void Test_Deserialization_WithDefaultValue()
    {
        var json = "{\"dictionary\":{\"key1\":10},\"defaultValue\":42,\"setDefaultValueBeforeSettingValue\":false}";
        DefaultDictionary<string, int>? defaultDict = JsonSerializer.Deserialize<DefaultDictionary<string, int>>(json);

        Assert.That(defaultDict, Is.Not.Null);
        Assert.That(defaultDict["key1"], Is.EqualTo(10));
        Assert.That(defaultDict["missingKey"], Is.EqualTo(42));
    }

    [Test]
    public void Test_Serialization_WithDefaultValueFactory()
    {
        DefaultDictionary<string, int> defaultDict = new(() => 100);
        defaultDict["key1"] = 10;

        var json = JsonSerializer.Serialize(defaultDict);
        Assert.That(json, Does.Contain("\"key1\":10"));
        Assert.That(json, Does.Contain("\"setDefaultValueBeforeSettingValue\":false"));
        Assert.That(json,
            Does.Contain(
                "\"serializedDefaultValueFactory\":\"{\\u0022__type\\u0022:\\u0022LambdaExpressionNode:#Serialize.Linq.Nodes\\u0022,\\u0022NodeType\\u0022:18,\\u0022Type\\u0022:{\\u0022GenericArguments\\u0022:[{\\u0022Name\\u0022:\\u0022System.Int32\\u0022}],\\u0022Name\\u0022:\\u0022System.Func\\u00601\\u0022},\\u0022Body\\u0022:{\\u0022__type\\u0022:\\u0022ConstantExpressionNode:#Serialize.Linq.Nodes\\u0022,\\u0022NodeType\\u0022:9,\\u0022Type\\u0022:{\\u0022Name\\u0022:\\u0022System.Int32\\u0022},\\u0022Value\\u0022:100},\\u0022Parameters\\u0022:[]}\""));
    }

    [Test]
    public void Test_Deserialization_WithDefaultValueFactory()
    {
        var json =
            "{\"dictionary\":{\"key1\":10},\"defaultValue\":null,\"serializedDefaultValueFactory\":\"{\\u0022__type\\u0022:\\u0022LambdaExpressionNode:#Serialize.Linq.Nodes\\u0022,\\u0022NodeType\\u0022:18,\\u0022Type\\u0022:{\\u0022GenericArguments\\u0022:[{\\u0022Name\\u0022:\\u0022System.Int32\\u0022}],\\u0022Name\\u0022:\\u0022System.Func\\u00601\\u0022},\\u0022Body\\u0022:{\\u0022__type\\u0022:\\u0022ConstantExpressionNode:#Serialize.Linq.Nodes\\u0022,\\u0022NodeType\\u0022:9,\\u0022Type\\u0022:{\\u0022Name\\u0022:\\u0022System.Int32\\u0022},\\u0022Value\\u0022:100},\\u0022Parameters\\u0022:[]}\",\"setDefaultValueBeforeSettingValue\":false}";
        DefaultDictionary<string, int>? defaultDict = JsonSerializer.Deserialize<DefaultDictionary<string, int>>(json);

        Assert.That(defaultDict, Is.Not.Null);
        Assert.That(defaultDict.IsDefaultValueFactorySet, Is.True);
        Assert.That(defaultDict["key1"], Is.EqualTo(10));
        Assert.That(defaultDict["missingKey"], Is.EqualTo(100));
    }
}
