using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    internal class RuleEngineConfiguration<T> : IConfiguration<T>
    {
        private readonly IConfiguration<T> _configuration;

        public Guid RuleEngineId { get; set; }

        public RuleEngineConfiguration(IConfiguration<T> configuration)
        {
            _configuration = configuration;
        }

        public int? ExecutionOrder
        {
            get { return _configuration.ExecutionOrder; }

            set { _configuration.ExecutionOrder = value; }
        }

        public bool Skip
        {
            get { return _configuration.Skip; }

            set { _configuration.Skip = value; }
        }

        public bool? Terminate
        {
            get { return _configuration.Terminate; }

            set { _configuration.Terminate = value; }
        }

        Expression<Predicate<T>> IConfiguration<T>.Constraint
        {
            get { return _configuration.Constraint; }

            set { _configuration.Constraint = value; }
        }
    }
}