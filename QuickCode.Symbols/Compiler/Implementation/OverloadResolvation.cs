using QuickCode.Symbols.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace QuickCode.Symbols.Compiler.Implementation;

static class OverloadResolvation
{

    public static int? GetConversionCost(ITypeSymbol from, ITypeSymbol to)
    {
        if (from == to) return 0; // Exact match
                                  // TODO: Add real type system checks here (implicit, base types, interfaces, etc.)
        return null; // null for "not assignable"
    }
    public static T? ResolveBestMethod<T>(ArgumentInfo[] args, IEnumerable<T> candidates) where T : class, IVaryParametersFuncBaseSymbol
    {
        var scoredCandidates = new List<(T Method, int Score)>();

        foreach (var method in candidates)
        {
            var parameters = method.Parameters;
            var parameterMap = new Dictionary<string, int>(); // name -> index
            for (int i = 0; i < parameters.Length; i++)
                parameterMap[parameters[i].Name] = i;

            var matched = new bool[parameters.Length];

            int totalScore = 0;

            // Step 1: Match positional arguments
            int positionalCount = args.TakeWhile(a => a.Name is null).Count();
            if (positionalCount > parameters.Length)
                continue;

            for (int i = 0; i < positionalCount; i++)
            {
                var paramIndex = i;
                var arg = args[i];
                var paramType = parameters[paramIndex].Type;

                int? cost = GetConversionCost(arg.Type, paramType);
                if (cost is null)
                {
                    goto incompatible;
                }

                totalScore += cost.Value;
                matched[paramIndex] = true;
            }

            // Step 2: Match named arguments
            for (int i = positionalCount; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.Name is null)
                {
                    goto incompatible;
                }

                if (!parameterMap.TryGetValue(arg.Name, out int paramIndex))
                {
                    goto incompatible;
                }

                if (matched[paramIndex])
                {
                    goto incompatible; // duplicate
                }

                var paramType = parameters[paramIndex].Type;
                int? cost = GetConversionCost(arg.Type, paramType);
                if (cost is null)
                {
                    goto incompatible;
                }

                totalScore += cost.Value;
                matched[paramIndex] = true;
            }


            // Step 3: Check unmatched parameters: they must be optional
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!matched[i])
                {
                    if (!parameters[i].HasDefaultParameter)
                    {
                        goto incompatible;
                    }
                    // Optional parameters add no cost
                }
            }

            
            scoredCandidates.Add((method, totalScore));
        incompatible:
            ;
        }

        // Select best candidate
        if (scoredCandidates.Count == 0) return null;

        var best = scoredCandidates.OrderBy(x => x.Score).ToList();

        if (best.Count >= 2 && best[0].Score == best[1].Score)
            throw new Exception("Ambiguous method call");

        return best[0].Method;
    }
    public static IBinaryOperatorFuncSymbol? ResolveBestMethod(ITypeSymbol left, ITypeSymbol right, IEnumerable<IBinaryOperatorFuncSymbol> candidates)
    {
        var scoredCandidates = new List<(IBinaryOperatorFuncSymbol Method, int Score)>();

        foreach (var method in candidates)
        {
            // Step 1: Match positional arguments

            int? leftCost = GetConversionCost(left, method.LeftInputType);
            if (leftCost is null)
            {
                goto incompatible;
            }
            int? rightCost = GetConversionCost(right, method.LeftInputType);
            if (rightCost is null)
            {
                goto incompatible;
            }
            scoredCandidates.Add((method, leftCost.Value + rightCost.Value));
        incompatible:
            ;
        }

        // Select best candidate
        if (scoredCandidates.Count == 0) return null;

        var best = scoredCandidates.OrderBy(x => x.Score).ToList();

        if (best.Count >= 2 && best[0].Score == best[1].Score)
            throw new Exception("Ambiguous method call");

        return best[0].Method;
    }
    public static T? ResolveBestMethod<T>(ITypeSymbol param, IEnumerable<T> candidates) where T : class, IUnaryOperatorBaseFuncSymbol
    {
        var scoredCandidates = new List<(T Method, int Score)>();

        foreach (var method in candidates)
        {

            // Step 1: Match positional arguments

            int? cost = GetConversionCost(param, method.InputType);
            if (cost is null)
            {
                goto incompatible;
            }
            scoredCandidates.Add((method, cost.Value));
        incompatible:
            ;
        }

        // Select best candidate
        if (scoredCandidates.Count == 0) return null;

        var best = scoredCandidates.OrderBy(x => x.Score).ToList();

        if (best.Count >= 2 && best[0].Score == best[1].Score)
            throw new Exception("Ambiguous method call");

        return best[0].Method;
    }
}
