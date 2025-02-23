﻿#region Copyright(c) Alexey Sadomov, Vladimir Timashkov. All Rights Reserved.
// -----------------------------------------------------------------------------
// Copyright(c) 2010 Alexey Sadomov, Vladimir Timashkov. All Rights Reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//   1. No Trademark License - Microsoft Public License (Ms-PL) does not grant you rights to use
//      authors names, logos, or trademarks.
//   2. If you distribute any portion of the software, you must retain all copyright,
//      patent, trademark, and attribution notices that are present in the software.
//   3. If you distribute any portion of the software in source code form, you may do
//      so only under this license by including a complete copy of Microsoft Public License (Ms-PL)
//      with your distribution. If you distribute any portion of the software in compiled
//      or object code form, you may only do so under a license that complies with
//      Microsoft Public License (Ms-PL).
//   4. The names of the authors may not be used to endorse or promote products
//      derived from this software without specific prior written permission.
//
// The software is licensed "as-is." You bear the risk of using it. The authors
// give no express warranties, guarantees or conditions. You may have additional consumer
// rights under your local laws which this license cannot change. To the extent permitted
// under your local laws, the authors exclude the implied warranties of merchantability,
// fitness for a particular purpose and non-infringement.
// -----------------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using CamlexNET.Impl.Operands;
using CamlexNET.Interfaces;

namespace CamlexNET.Impl.Operations.Join
{
    internal class JoinAnalyzer : BaseAnalyzer
    {
        protected IOperandBuilder operandBuilder;
        public JoinAnalyzer(IOperationResultBuilder operationResultBuilder, IOperandBuilder operandBuilder)
            : base(operationResultBuilder)
        {
            this.operandBuilder = operandBuilder;
        }

        public override bool IsValid(LambdaExpression expr)
        {
            if (expr == null)
            {
                return false;
            }
            // body should be MethodCallExpression
            if (!(expr.Body is MethodCallExpression))
            {
                return false;
            }
            var body = expr.Body as MethodCallExpression;
            return this.isValid(body, ReflectionHelper.ForeignListMethodName);
        }

        private bool isValid(MethodCallExpression body, string methodName)
        {
            if (body.Method.Name != methodName)
            {
                return false;
            }

            if (body.Arguments.Count != 2)
            {
                return false;
            }

            if (body.Arguments[0].NodeType != ExpressionType.Call)
            {
                return false;
            }

            var leftExpression = body.Arguments[0] as MethodCallExpression;
            if (!this.isValid(leftExpression, ReflectionHelper.PrimaryListMethodName))
            {
                if (!this.isValidLeftExpression(body.Arguments[0]))
                {
                    return false;
                }
            }

            var argumentExpression = body.Arguments[1];
            if (!this.isValidEvaluableExpression(argumentExpression) || argumentExpression.Type != typeof(string))
            {
                return false;
            }

            return true;
        }

        protected bool isValidLeftExpression(Expression leftExpression)
        {
            if (!(leftExpression is MethodCallExpression))
            {
                return false;
            }
            var leftOperand = leftExpression as MethodCallExpression;
            if (leftOperand.Method.Name != ReflectionHelper.IndexerMethodName)
            {
                return false;
            }

            if (leftOperand.Arguments.Count != 1)
            {
                return false;
            }

            // parameter of indexer can be constant, variable or method call
            var argumentExpression = leftOperand.Arguments[0];
            if (!this.isValidEvaluableExpression(argumentExpression))
            {
                return false;
            }

            // type of argument expression should be string
            return (argumentExpression.Type == typeof(string));
        }

        public override IOperation GetOperation(LambdaExpression expr)
        {
            throw new NotSupportedException();
        }

        public override IOperation GetOperation(LambdaExpression expr, object param)
        {
            if (!IsValid(expr))
            {
                throw new NonSupportedExpressionException(expr);
            }
            var fieldRefOperand = this.getFieldRefOperand(expr);
            var valueOperand = this.getValueOperand(expr);
            return new JoinOperation(operationResultBuilder, fieldRefOperand, valueOperand, (JoinType)param);
        }

        private IOperand getValueOperand(LambdaExpression expr)
        {
            var body = expr.Body as MethodCallExpression;
            return this.operandBuilder.CreateFieldRefOperandForJoin(body.Arguments[1]);
        }

        private IOperand getFieldRefOperand(LambdaExpression expr)
        {
            var body = expr.Body as MethodCallExpression;
            var leftExpression = body.Arguments[0] as MethodCallExpression;
            if (leftExpression.Method.Name == ReflectionHelper.PrimaryListMethodName)
            {
                // both primary and foreign lists are specified
                return operandBuilder.CreateFieldRefOperandForJoin(leftExpression.Arguments[0], leftExpression.Arguments[1]);
            }

            // only foreign list is specified
            var fieldRef = (FieldRefOperand)operandBuilder.CreateFieldRefOperand(body.Arguments[0], null);
            fieldRef.Attributes = new List<KeyValuePair<string, string>>(new[]{new KeyValuePair<string, string>(Attributes.RefType, Values.Id)});
            return fieldRef;
        }
    }
}


