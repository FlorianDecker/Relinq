// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Utilities;

namespace Remotion.Data.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class AllResultOperatorTest
  {
    private AllResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (t => t > 2);
      _resultOperator = new AllResultOperator (predicate);
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (AllResultOperator)));
      Assert.That (((AllResultOperator) clone).Predicate, Is.SameAs (_resultOperator.Predicate));
    }

    [Test]
    public void ExecuteInMemory_True ()
    {
      IEnumerable items = new[] { 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.True);
    }

    [Test]
    public void ExecuteInMemory_False ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.False);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Cannot execute the result operator 'All(i => (i > [main]))' in memory because the Predicate cannot be evaluated.")]
    public void ExecuteInMemory_ExpressionNotCompilable ()
    {
      var querySourceReference = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());

      var predicateParameter = Expression.Parameter (typeof (int), "i");
      var predicate = Expression.Lambda (
          Expression.MakeBinary (ExpressionType.GreaterThan, predicateParameter, querySourceReference), 
          predicateParameter);
      _resultOperator = new AllResultOperator (predicate);

      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      _resultOperator.ExecuteInMemory<int> (input);

      Assert.Fail ();
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (bool)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Student));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var resultOperator = new AllResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.Predicate, Is.SameAs (newExpression));
    }

    [Test]
    public new void ToString ()
    {
      var querySource = ExpressionHelper.CreateMainFromClause_Int ("x", typeof (int), ExpressionHelper.CreateIntQueryable());
      var querySourceReference = new QuerySourceReferenceExpression (querySource);
      
      var predicateParameter = Expression.Parameter (typeof (int), "i");
      var predicate = Expression.Lambda (
          Expression.MakeBinary (ExpressionType.GreaterThan, predicateParameter, querySourceReference), 
          predicateParameter);
      
      var resultOperator = new AllResultOperator (predicate);
      Assert.That (resultOperator.ToString (), Is.EqualTo ("All(i => (i > [x]))"));
    }
  }
}