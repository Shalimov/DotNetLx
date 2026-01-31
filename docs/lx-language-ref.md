# Lx Language Reference

Lx is a compact, expression-first language with sharp edges and clear intent. It favors small, explicit syntax over magical inference, and its runtime stays honest: no silent coercions beyond string concatenation, no implicit returns except in lambdas.

This reference is derived from the grammar in `DotNetLxInterpreter/FrontEnd/language.grammar` and the behavior implemented by the scanner, parser, static analyzer, and interpreter.

**Lexical Structure**
- **Identifiers**: ASCII letters, digits, and `_`, starting with a letter or `_`.
- **Keywords**: `and`, `break`, `class`, `else`, `false`, `for`, `fun`, `if`, `nil`, `or`, `print`, `return`, `static`, `super`, `this`, `true`, `var`, `while`.
- **Literals**: numbers (double), strings (double-quoted), `true`, `false`, `nil`.
- **Comments**: `//` to end of line, `/* ... */` multiline with nesting.
- **Tokens not yet in syntax**: `|>` is tokenized but unused in the grammar.

**Program Structure**
```
program        -> declaration* EOF ;
declaration    -> classDecl | varDecl | funDecl | statement ;
```

**Declarations**
```
varDecl        -> "var" IDENTIFIER ( "=" expression )? ";" ;
funDecl        -> "fun" function ;
classDecl      -> "class" IDENTIFIER ( "<" IDENTIFIER )? "{" ( "static"? function | property )* "}" ;
function       -> IDENTIFIER "(" parameters? ")" blockStmt ;
property       -> IDENTIFIER blockStmt ;
```

Examples:
```lx
var answer = 42;

fun add(a, b) {
  return a + b;
}

class Pair {
  init(a, b) {
    this.a = a;
    this.b = b;
  }

  sum {
    return this.a + this.b;
  }

  static zero() {
    return Pair(0, 0);
  }
}
```

**Statements**
```
statement      -> exprStmt | ifStmt | forStmt | whileStmt | breakStmt
               | returnStmt | printStmt | blockStmt ;
ifStmt         -> "if" "(" expression ")" statement ( "else" statement )? ;
forStmt        -> "for" "(" ( varDecl | exprStmt | ";" ) expression ";" expression? ")" statement ;
whileStmt      -> "while" "(" expression ")" statement ;
blockStmt      -> "{" declaration* "}" ;
breakStmt      -> "break" ";" ;
returnStmt     -> "return" expression? ";" ;
printStmt      -> "print" expression ";" ;
exprStmt       -> expression ";" ;
```

Notes:
- `for` is desugared to a `while` with optional initializer and increment.
- `break` is valid only inside loop bodies.
- `return` is valid only inside functions/methods; `init` cannot return a value.

**Expressions and Precedence**
```
expression     -> commaseq ;
commaseq       -> assignment ( "," assignment )* ;
assignment     -> ( call "." )? IDENTIFIER "=" assignment | logicOr ;
logicOr        -> logicAnd "or" logicOr ;
logicAnd       -> ternary "and" ternary ;
ternary        -> equality ( "?" ternary ":" ternary )? ;
equality       -> comparison ( ( "!=" | "==" ) comparison )* ;
comparison     -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           -> factor ( ( "-" | "+" ) factor )* ;
factor         -> unary ( ( "/" | "*" ) unary )* ;
unary          -> ( "!" | "-" ) unary | call ;
call           -> primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
arguments      -> assignment ( "," assignment )* ;
primary        -> NUMBER | STRING | "true" | "false" | "nil"
               | "(" expression ")" | IDENTIFIER | lambda | super | "this" ;
lambda         -> "fun" "(" parameters? ")" ( blockStmt | expression ) ;
super          -> "super" "." IDENTIFIER ;
```

Operator notes:
- `,` returns the right operand value after evaluating both.
- `and`/`or` short-circuit and return the last evaluated operand.
- Unary supports only `!` and `-`; other unary uses are rejected.

**Values and Truthiness**
- `nil` and `false` are falsy; everything else is truthy.
- Numbers are doubles; comparisons require matching types.
- `+` adds numbers or concatenates if either operand is a string.

**Functions and Lambdas**
- Functions are first-class and close over lexical scope.
- Lambdas can be block or expression bodies; expression bodies are desugared into `return`.
- Arity is strict; calls must match parameter count exactly (max 255).

Examples:
```lx
fun thrice(fn) {
  for (var i = 1; i <= 3; i = i + 1) fn(i);
}

thrice(fun (x) print x);

fun square(x) x * x;
print square(5);
```

**Classes, Instances, and Inheritance**
- Class calls construct instances; `init` runs automatically if present.
- Methods are looked up through the inheritance chain.
- `this` is valid only inside class methods.
- `super` is valid only inside subclasses and binds to the superclass method.
- Property blocks (`name { ... }`) are getters: access calls them with no args.
- `static` methods belong to the class object (its metaclass) and are called on the class.

Example:
```lx
class A {
  greet() { print "A"; }
}

class B < A {
  greet() {
    print "B";
    super.greet();
  }
}

var b = B();
b.greet();
```

**Variables and Scope**
- `var` declares a variable in the current scope.
- Variables must be initialized before first read; reading an uninitialized local is a runtime error.
- The analyzer rejects reading a variable in its own initializer.
- Closures capture by lexical scope; shadowing is allowed.

**Static Analysis Rules**
- `break` outside a loop is an error.
- `return` outside callable contexts is an error.
- `return` with a value inside `init` is an error.
- `this` outside a class is an error.
- `super` outside a subclass is an error.
- Unused local variables (including parameters) are reported when scopes end.

**Built-ins**
- `print expr;` writes the stringified value.
- `clock()` returns Unix time in seconds.

**Errors and Assertions**
- Numeric operators require numeric operands.
- Comparisons require operands of the same type.
- Division by zero is a runtime error.
- Calling non-callables is a runtime error.
- Accessing unknown fields on instances is a runtime error.
