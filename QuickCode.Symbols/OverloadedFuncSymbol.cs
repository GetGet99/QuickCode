//using System.Collections.Immutable;

//namespace QuickCode.Symbols;

//public abstract record class
//    OverloadedFuncSymbol(
//    Dictionary<int, (List<SingleFuncSymbol> ExactParameterCountOverloads,
//        List<SingleFuncSymbol> OptionalParameterCountOverloads)> Overloads
//) : Symbol
//{
//    public void Push(SingleFuncSymbol newOverload)
//    {
//        // optional parameters not yet implemented
//        if (!Overloads.TryGetValue(newOverload.Parameters.Length, out var ov))
//        {
//            ov = ([], []);
//            goto Insert;
//        }
//        // verify
        
//    Insert:
//        ov.ExactParameterCountOverloads.Add(newOverload);
//    }
//    public SingleFuncSymbol? Get(ImmutableArray<TypeSymbol> @params)
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
//        NextFunction:
//            ;
//        }
//    }
//}