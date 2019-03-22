﻿using CouchDB.Client.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CouchDB.Client
{
    internal partial class QueryTranslator
    {
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        protected override Expression VisitExtension(Expression x)
        {
            return x;
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            // Queryable
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                if (m.Method.Name == "Where")
                    return VisitWhereMethod(m);
                else if (m.Method.Name == "OrderBy" || m.Method.Name == "ThenBy")
                    return VisitOrderAscendingMethod(m);
                else if (m.Method.Name == "OrderByDescending" || m.Method.Name == "ThenByDescending")
                    return VisitOrderDescendingMethod(m);
                else if (m.Method.Name == "Skip")
                    return VisitSkipMethod(m);
                else if (m.Method.Name == "Take")
                    return VisitTakeMethod(m);
                else if (m.Method.Name == "Select")
                    return VisitSelectMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(QueryableExtensions))
            {
                if (m.Method.Name == "UseBookmark")
                    return VisitUseBookmarkMethod(m);
                else if (m.Method.Name == "WithReadQuorum")
                    return VisitWithQuorumMethod(m);
                else if (m.Method.Name == "UpdateIndex")
                    return VisitUpdateIndexMethod(m);
                else if (m.Method.Name == "FromStable")
                    return VisitFromStableMethod(m);
                else if (m.Method.Name == "UseIndex")
                    return VisitUseIndexMethod(m);
            }
            // Not Queryable
            else
            {
                if (m.Method.Name == "All")
                    return VisitAnyMethod(m);
                else if (m.Method.Name == "Any")
                    return VisitAllMethod(m);
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        #region Queryable

        private Expression VisitWhereMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"selector\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append(",");
            return m;
        }
        private Expression VisitOrderAscendingMethod(MethodCallExpression m)
        {
            void InspectOrdering(Expression e)
            {
                var o = e as MethodCallExpression;
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);

                if (o.Method.Name == "OrderBy")
                {
                    this.Visit(o.Arguments[0]);
                    sb.Append("\"sort\":[");
                    this.Visit(lambda.Body);
                }
                else if (o.Method.Name == "OrderByDescending")
                {
                    throw new InvalidOperationException("Cannot order in different directions.");
                }
                else if (o.Method.Name == "ThenBy")
                {
                    InspectOrdering(o.Arguments[0]);
                    this.Visit(lambda.Body);
                }
                else
                    return;
                sb.Append(",");
            }

            InspectOrdering(m);
            sb.Append("],");
            return m;
        }
        private Expression VisitOrderDescendingMethod(MethodCallExpression m)
        {
            void InspectOrdering(Expression e)
            {
                var o = e as MethodCallExpression;
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);

                if (o.Method.Name == "OrderBy")
                {
                    throw new InvalidOperationException("Cannot order in different directions.");
                }
                else if (o.Method.Name == "OrderByDescending")
                {
                    this.Visit(o.Arguments[0]);
                    sb.Append("\"sort\":[");
                    sb.Append("{");
                    this.Visit(lambda.Body);
                    sb.Append(":\"desc\"}");
                }
                else if (o.Method.Name == "ThenByDescending")
                {
                    InspectOrdering(o.Arguments[0]);
                    sb.Append("{");
                    this.Visit(lambda.Body);
                    sb.Append(":\"desc\"}");
                }
                else
                    return;
                sb.Append(",");
            }

            InspectOrdering(m);
            sb.Append("],");
            return m;
        }
        private Expression VisitSkipMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append($"\"limit\":{m.Arguments[1]},");
            return m;
        }
        private Expression VisitTakeMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append($"\"skip\":{m.Arguments[1]},");
            return m;
        }
        private Expression VisitSelectMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"fields\":[");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            var n = lambda.Body as NewExpression;

            if (n == null)
                throw new NotSupportedException($"The expression of type {lambda.Body.GetType()} is not supported in the Select method.");

            foreach (var a in n.Arguments)
            {
                this.Visit(a);
                sb.Append(",");
            }
            sb.Append("],");

            return m;
        }

        #endregion

        #region QueryableExtensions

        private Expression VisitUseBookmarkMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"bookmark\":");
            this.Visit(m.Arguments[1]);
            sb.Append(",");
            return m;
        }
        private Expression VisitWithQuorumMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"r\":");
            this.Visit(m.Arguments[1]);
            sb.Append(",");
            return m;
        }
        private Expression VisitUpdateIndexMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"update\":");
            this.Visit(m.Arguments[1]);
            sb.Append(",");
            return m;
        }
        private Expression VisitFromStableMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"stable\":");
            this.Visit(m.Arguments[1]);
            sb.Append(",");
            return m;
        }
        private Expression VisitUseIndexMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"use_index\":");
            this.Visit(m.Arguments[1]);
            sb.Append(",");
            return m;
        }


        #endregion

        #region NotQueryable

        private Expression VisitAnyMethod(MethodCallExpression m)
        {
            sb.Append("{");
            this.Visit(m.Arguments[0]);
            sb.Append(":{\"$elemMatch\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append("}}");
            return m;
        }
        private Expression VisitAllMethod(MethodCallExpression m)
        {
            sb.Append("{");
            this.Visit(m.Arguments[0]);
            sb.Append(":{\"$allMatch\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append("}}");
            return m;
        }

        #endregion
    }
}