namespace Tinkoff.ISA.DAL.Elasticsearch
{
    // This class contains name of the indexes in elasticsearch
    // make sure that keys from IndexSearchParams in *.json files match names here
    internal class Indexes
    {
        public const string QuestionsIndex = "questions";
        public const string AnswersIndex = "answers";
        public const string ConfluenceIndex = "confluence";
        public const string JiraIndex = "jira";
    }
}
