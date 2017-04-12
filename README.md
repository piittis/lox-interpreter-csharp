# Lox-Interpreter-CSharp

My C# implementation for the tree-walk interpreter from http://www.craftinginterpreters.com/

Work in progress.

Current state:
- Scan, parse and interpret expressions.

#### Grammar:
```
expression → binary_err
binary_err → ("!=" | "==" | ">" | ">=" | "<" | "<=" | "+" | "/" | "*") comma
           | comma
comma      → ternary ( "," ternary )*
ternary    → equality ( "?" expression ":" expression )?
equality   → comparison ( ( "!=" | "==" ) comparison )*
comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )*
term       → factor ( ( "-" | "+" ) factor )*
factor     → unary ( ( "/" | "*" ) unary )*
unary      → ( "!" | "-" ) unary
           | primary
primary    → NUMBER | STRING | "false" | "true" | "nil"
           | "(" expression ")"
```
