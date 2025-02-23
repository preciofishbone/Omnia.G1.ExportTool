﻿#region Copyright(c) Alexey Sadomov, Vladimir Timashkov, Stef Heyenrath. All Rights Reserved.
// -----------------------------------------------------------------------------
// Copyright(c) 2010 Alexey Sadomov, Vladimir Timashkov, Stef Heyenrath. All Rights Reserved.
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
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using CamlexNET.Impl.Operations.Results;
using CamlexNET.Interfaces;
using Microsoft.SharePoint.Client;

namespace CamlexNET.Impl
{
	internal class GenericTranslator : ITranslator
	{
		private readonly IAnalyzer analyzer;

		public GenericTranslator(IAnalyzer analyzer)
		{
			this.analyzer = analyzer;
		}

		public XElement TranslateWhere(LambdaExpression expr)
		{
			if (!this.analyzer.IsValid(expr))
			{
				throw new NonSupportedExpressionException(expr);
			}

			var operation = this.analyzer.GetOperation(expr);
			var operationCaml = operation.ToResult().Value;

			return new XElement(Tags.Where, operationCaml);
		}

		public XElement TranslateOrderBy(LambdaExpression expr)
		{
			if (!this.analyzer.IsValid(expr))
			{
				throw new NonSupportedExpressionException(expr);
			}

			var operation = this.analyzer.GetOperation(expr);
			var result = (XElementArrayOperationResult)operation.ToResult();

			return new XElement(Tags.OrderBy, result.Value);
		}

		public XElement TranslateGroupBy(LambdaExpression expr, bool? collapse, int? groupLimit)
		{
			if (!this.analyzer.IsValid(expr))
			{
				throw new NonSupportedExpressionException(expr);
			}

			var operation = this.analyzer.GetOperation(expr);
			var result = (XElementArrayOperationResult)operation.ToResult();

			var caml = new XElement(Tags.GroupBy, result.Value);
			if (collapse != null)
			{
				caml.SetAttributeValue(Attributes.Collapse, collapse.Value.ToString());
			}
			if (groupLimit != null)
			{
				caml.SetAttributeValue(Attributes.GroupLimit, groupLimit.Value);
			}

			return caml;
		}

		public XElement TranslateViewFields(Expression<Func<ListItem, object[]>> expr)
		{
			if (!this.analyzer.IsValid(expr))
			{
				throw new NonSupportedExpressionException(expr);
			}

			var operation = this.analyzer.GetOperation(expr);
			var result = (XElementArrayOperationResult)operation.ToResult();

			return new XElement(Tags.ViewFields, result.Value);
		}

	    public XElement TranslateRowLimit(LambdaExpression expr)
	    {
            if (!this.analyzer.IsValid(expr))
            {
                throw new NonSupportedExpressionException(expr);
            }

            var operation = this.analyzer.GetOperation(expr);
            var operationCaml = operation.ToResult().Value;

            return (XElement)operationCaml;
	    }

        public XElement TranslateJoin(Expression<Func<ListItem, object>> expr, JoinType type)
        {
            if (!this.analyzer.IsValid(expr))
            {
                throw new NonSupportedExpressionException(expr);
            }

            var operation = this.analyzer.GetOperation(expr, type);
            var result = operation.ToResult();
            var element = (XElement)result.Value;

//            if (element.HasAttributes)
//            {
//                var attributes = element.Attributes().ToList();
//                attributes.Insert(0, new XAttribute(Attributes.Type, type.ToString().ToUpper()));
//                element.RemoveAttributes();
//                element.Add(attributes);
//            }
//            else
//            {
//                element.SetAttributeValue(Attributes.Type, type.ToString().ToUpper());
//            }
            return element;
        }

        public XElement TranslateProjectedField(Expression<Func<ListItem, object>> expr)
        {
            if (!this.analyzer.IsValid(expr))
            {
                throw new NonSupportedExpressionException(expr);
            }

            var operation = this.analyzer.GetOperation(expr);
            var result = operation.ToResult();
            return (XElement)result.Value;
        }
	}
}
