using System;
using System.Collections.Generic;
using System.Linq;

namespace D20
{
    public abstract class BinaryOperator : Rollable
	{
	    protected enum Precedence
	    {
	        Sum,
	        Product,
	    }

		protected abstract int Apply(int left, int right);
		protected abstract double Apply(double left, double right);
		protected abstract string Symbol { get; }
	    protected  abstract Precedence OperatorPrecedence { get; }

		protected Rollable Left { get; }
		protected Rollable Right { get; }

		protected BinaryOperator(Rollable left, Rollable right)
		{
			this.Left = left;
			this.Right = right;
		}

		public override int MinValue => this.Apply(this.Left.MinValue, this.Right.MinValue);
		public override int MaxValue => this.Apply(this.Left.MaxValue, this.Right.MaxValue);
		public override double Average => this.Apply(this.Left.Average, this.Right.Average);

		public override IEnumerable<int> PossibleValues =>
		    JoinPossibleValues(this.Left.PossibleValues, this.Right.PossibleValues, this.Apply).OrderBy(x => x);

		public override IEnumerable<CountedValue> CountedValues =>
			JoinCountedValues(this.Left.CountedValues, this.Right.CountedValues, this.Apply).OrderBy(v => v.Value);

		public override int Roll() => this.Apply(this.Left.Roll(), this.Right.Roll());

	    public override string ToString()
	    {
	        var leftOperator = this.Left as BinaryOperator;
	        var rightOperator = this.Right as BinaryOperator;

	        var leftParenthesis = leftOperator?.OperatorPrecedence < this.OperatorPrecedence;
	        var rightParenthesis = rightOperator?.OperatorPrecedence < this.OperatorPrecedence;

	        if (leftParenthesis && rightParenthesis)
	            return $"({this.Left}){this.Symbol}({this.Right})";
	        if (leftParenthesis)
	            return $"({this.Left}){this.Symbol}{this.Right}";
	        if (rightParenthesis)
	            return $"{this.Left}{this.Symbol}({this.Right})";
	        return $"{this.Left}{this.Symbol}{this.Right}";
	    }

	    public static IEnumerable<int> JoinPossibleValues(
	            IEnumerable<int> left, IEnumerable<int> right, Func<int, int, int> @operator)
	        => left.Join(right, @operator).Distinct();

	    public static IEnumerable<CountedValue> JoinCountedValues(
	            IEnumerable<CountedValue> left, IEnumerable<CountedValue> right, Func<int, int, int> @operator)
	        => left.Join(right, (l, r) => new CountedValue(@operator(l.Value, r.Value), l.Count * r.Count))
	            .GroupBy(x => x.Value).Select(CountedValue.FromGroup);
	}
}