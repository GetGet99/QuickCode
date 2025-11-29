using Get.LangSupport;
using QuickCode;

var metadata = new TextmateGrammarMetadata
{
    LanguageId = "testlang",
    LanguageExtensions = [".testlang"]
};

//string contrib = metadata.GetContributionsJSON();
string grammar = metadata.GetGrammarJSON(TextmateGrammarGenerator.GenerateRepository<QuickCodeLexer>());

Console.WriteLine(grammar);
