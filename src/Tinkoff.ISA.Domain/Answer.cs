using System;

namespace Tinkoff.ISA.Domain
{
    public class Answer
    {
        public Guid Id { get; set; }

        public string Text { get; set; }

        public int Rank { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
