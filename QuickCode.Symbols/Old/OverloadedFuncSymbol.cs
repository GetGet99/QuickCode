//using QuickCode.Symbols.Types;
//using System.Collections.Immutable;

//namespace QuickCode.Symbols;

//public record class
//    OverloadedFuncSymbol(
//        Dictionary<int, (List<SingleFuncSymbol> ExactParameterCountOverloads,
//        List<SingleFuncSymbol> OptionalParameterCountOverloads)> Overloads
//) : IFuncSymbol
//{
//    public OverloadedFuncSymbol() : this(Overloads: []) { }
//    public OverloadedFuncSymbol(IEnumerable<SingleFuncSymbol> funcs) : this()
//    {
//        foreach (var func in funcs)
//        {
//            Push(func);
//        }
//    }
//    public bool Push(SingleFuncSymbol newOverload)
//    {
//        // optional parameters not yet implemented
//        if (!Overloads.TryGetValue(newOverload.Parameters.Length, out var ov))
//        {
//            Overloads[newOverload.Parameters.Length] = ov = ([], []);
//            goto Insert;
//        }
//        // verify that there's no exact match
//        foreach (var func in ov.ExactParameterCountOverloads)
//        {
//            // case of optional parameter
//            if (func.Parameters.Length != newOverload.Parameters.Length)
//                continue;
//            for (int i = 0; i < func.Parameters.Length; i++)
//            {
//                if (newOverload.Parameters[i].Type != func.Parameters[i].Type)
//                {
//                    goto NextFunction;
//                }
//            }
//            // there's exact match
//            return false;
//        NextFunction:
//            ;
//        }
//    Insert:
//        ov.ExactParameterCountOverloads.Add(newOverload);
//        return true;
//    }
//    public void PushOverride(SingleFuncSymbol newOverload)
//    {
//        // optional parameters not yet implemented
//        if (!Overloads.TryGetValue(newOverload.Parameters.Length, out var ov))
//        {
//            Overloads[newOverload.Parameters.Length] = ov = ([], []);
//            goto Insert;
//        }
//        // verify that there's no exact match
//    retry:
//        foreach (var func in ov.ExactParameterCountOverloads)
//        {
//            // case of optional parameter
//            if (func.Parameters.Length != newOverload.Parameters.Length)
//                continue;
//            for (int i = 0; i < func.Parameters.Length; i++)
//            {
//                if (newOverload.Parameters[i].Type != func.Parameters[i].Type)
//                {
//                    goto NextFunction;
//                }
//            }
//            // there's exact match
//            ov.ExactParameterCountOverloads.Remove(func);
//            goto retry;
//        NextFunction:
//            ;
//        }
//    Insert:
//        ov.ExactParameterCountOverloads.Add(newOverload);
//    }
//    public SingleFuncSymbol? Get(ImmutableArray<ITypeSymbol> @params)
//    {
//        // optional parameters not yet implemented
//        if (!Overloads.TryGetValue(@params.Length, out var ov))
//        {
//            return null;
//        }
//        // phase 1: exact
//        foreach (var func in ov.ExactParameterCountOverloads)
//        {
//            for (int i = 0; i < func.Parameters.Length; i++)
//            {
//                if (@params[i] != func.Parameters[i].Type)
//                {
//                    goto NextFunction;
//                }
//            }
//            return func;
//        NextFunction:
//            ;
//        }
//        // phase 2: exact + optional parameter, not implemented
//        // phase 3: implicit, not implemented
//        // phase 4: implicit + optional parameter, not implemented
//        return null;
//    }
//    public OverloadedFuncSymbol DeepClone()
//    {
//        Dictionary<int, (List<SingleFuncSymbol> ExactParameterCountOverloads,
//        List<SingleFuncSymbol> OptionalParameterCountOverloads)> clonedDict = new(Overloads.Capacity);
//        foreach (var (k, v) in Overloads)
//        {
//            clonedDict[k] = v;
//        }
//        return new(clonedDict);
//    }
//}