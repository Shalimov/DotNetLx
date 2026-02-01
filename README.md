# Language Reference

See [docs/lx-language-ref.md](docs/lx-language-ref.md) for the full specification.
Examples and test scripts live in `DotNetLxInterpreter/LxScripts`.

| Feature | Notes |
| --- | --- |
| Types | Numbers, strings, booleans, nil; truthy semantics |
| Functions | First-class, lexical closures, strict arity |
| Control Flow | if/else, while, for (desugared), break, return |
| Classes | Methods, properties (getter blocks), static methods |
| Inheritance | Single inheritance with `super` method dispatch |
| Built-ins | `print` statement, `clock()` native function |
| Analyzer | Unused vars, invalid `break`, `return`, `this`, `super` |

# DotNetLx

DotNetLx is a C# interpreter for Lx, a compact dynamic language built as part of a Crafting Interpreters-style exploration.
Reference: https://craftinginterpreters.com/

This repo implements Part 2 of the book in .NET.
All challenges through Chapter 13 are completed.

Key implementation details:
- Frontend is a hand-written scanner and recursive descent parser with the grammar in `DotNetLxInterpreter/FrontEnd/language.grammar`.
- Middle ground is a static analyzer that resolves lexical scope, enforces control-flow rules, and reports unused locals.
- Runtime is a tree-walking interpreter with environments, closures, and a small native surface (`clock`).
- Classes are implemented with instances, bound methods, properties as zero-arg getters, and a metaclass for statics.

TODO:
- Sugar syntax for `each` loops (via iterator).
- Conventional class properties like `[iterator]`, `[toString]`.
