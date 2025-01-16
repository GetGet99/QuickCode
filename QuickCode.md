# QuickCode

QuickCode is an as-yet-to-be-formally-named programming language that can be compiled into MSIL. The current name is QuickCode, but is subject to change.

QuickCode is designed to simplify coding experiences, making it faster to code.

## General Syntax

QuickCode is an indentation-based language. Indentation is the key in this language.

The compiler will be released and open sourced once it reaches a point where the language becomes usable enough.
No guarantees that it will be completed. We will have italicized text to state which is not implemented yet.

As of now, the specs may change at any time.

```
for i in 0..5:
    Print(i)
Print(10)
```

In the code above, `Print(i)` is part of the for loop, while `Print(10)` is not.

## Files

### Top Level Statements

QuickCode top level statements is a script. The entry point does not need a `main` method. For example, this can be the entire file.

```
i := 0
i++
Print(i)
i++
Print(i)
```

The entry point of QuickCode program is a top level statements script.

### Non-script files

*Non-script files, classes, and structs are currently not yet implemented.*

Other than the top level statements file type, we will also have non-script files and classes.
Non-script files hold classes and structs that is put in a given namespace.

```
namespace QuickCode.Example:
    class SampleClass:
        @public 
        func CallMe():
            Print("Hello World")
```

## Syntaxes in detail

### Indentation

To determine the block structure, we uses the indentation to infer whether statements are part of, or not part of block. Each statements in the same level block must have the same amount of indentation.

Lines without statements may have any number of spaces.

Tab characters (`\t`) are not recommended to be used for indentation. It will always be treated equal to 4 spaces because that's what I use.
No UNIX rule exceptions. Sorry.

### Comments

```
// line comments
/* block comments */
```

#### Comments and Indentation

Comments that exists on lines without statements are treated the same as if that line has empty space (ie. that line can have
any amount of spaces for indentation)

```
i := 0
if i >= 0:
    j := 0
// line comment can begin here
                // also here
        /*
            block comment can begin here
    */
    Print(j)
```


At any time if the line begins with block comments and there exists a statement after the block comments, the indentation engine will count the indentation before the block comments. For example,

```
i := 0
j := 0
if i >= 0:
    if j >= 0:
        /* Block Comment
*/  j++
```
or
```
i := 0
j := 0
if i >= 0:
    if j >= 0:
        /* Block Comment
*/                  j++
```
is functionally equivalent to:
```
i := 0
j := 0
if i >= 0:
    if j >= 0:
        j++ // uses the start of block comment's indentation
```

## No opeartion

`nop` can be used to declare a no-operation. This may be used to bypass a requirement that blocks
must have at least a statement in it.

```
i := 0
if i >= 0:
    nop
do:
    nop
while true
```

Note that `nop` may or may not get translated into actual runtime instructions. For example,
```
nop
nop
nop
... // 10 million more copies
nop
```

The example above may generate a very small or very large executable or take very little time or may take a while to execute. No guarantees are made whether the instructions are actually ommitted when compiled or not.

## Todos and Ellipses

*Note: this is not yet implemented.*

`todo` and `...` is a token that, once a program executes until this point, will throw `NotImplementedException`

## If

QuickCode uses the following syntax for if statements.

```
i := 0
if i > 0:
    i++
else if i < 0:
    i--
else:
    i = 42
Print(i)
```

## Loops

There are main 3 kinds of loops in QuickCode. It supports `break`, `continue`.

```
while true:
    break
for i in 0..10:
    continue
do:
    Print(42)
while false
```

### Conditional break and continue

Breaks and continue may be conditional. For example,
```
for i in 0..30:
    // skips all odd number
    continue if i % 2 != 0
    // skips all number greater than 15
    break if i >= 15
    Print(i)
```
This is a shorthand form of
```
for i in 0..30:
    // skips all odd number
    if i % 2 != 0:
        continue
    // skips all number greater than 15
    if i >= 15:
        break
    Print(i)
```

### For loop variable rules

Variable `i` is declared with implicit type from the right hand side.
The variable `i` is only defined inside the scope, unless it has already been defined outside the scope with the same type.

For example,
```
i := 0
for i in 0..10:
    break if i == 5
Print(i) // 5
```

However, these codes are not valid QuickCode codes.
```
```
for i in 0..10:
    break if i == 5
Print(i) // invalid code: i is not defined in outter scope
```
i := true
for i in 0..10: // invalid code: i is of different type
    break if i == 5
Print(i)
```

## Exit Statement

QuickCode has `exit` statement. `exit` statement exits the innermost block

Exiting an if statement skips remaining statements in the if statement

```
i := 20
if i >= 10:
    Print(i) // 20
    exit
    i++ // does not run
Print(i) // 20
```

Exiting `while`, `do while`, or `for` loop has the same behavior as `break` statement.

### Exit on condition

`exit` on its own may not be very helpful. Therefore, this is where condition comes in.

```
i := 20
if i > 10:
    Print(i) // 20
    exit if i > 20 // skips the execution of the remaining statements in the if statement
    i++ // does not run
    Print(42) // does not run
Print(i) // 20
```

Note that unlike `break if condition` or `continue if condition`, `exit if condition` is not equivalent to
simply using it with a regular if statement to check condition.
```
i := 20
if i > 10:
    Print(i) // 20
    if i > 20:
        // exits "if i > 20" block, not "if i > 10" block
        // so basically, "exit" statement here acts like a no-op.
        exit 
    i++ // does not run
    Print(42) // does not run
Print(i) // 20
```

## Labels, Gotos, and advanced control flow

Labels in QuickCode can be defined in two ways.

1. via un-indenting
```
func f:
    i := 0
$label1:
    Print(i)
    i++
    goto $label1 if i < 10
```

2. via `deflb` statement
```
func f:
    i := 0
    deflb $label1
    Print(i)
    i++
    goto $label1 if i < 10
```

Labels name must have `$` prefix and the rest of the name is a valid QuickCode Identifier.

*Note: The current implementation allows space between `$` character and the identifer name.*
*For example, `deflb $ HelloWorld` is allowed. Spaces between identifier and `$` is truncted out,*
*so `$  HelloWorld` and `$HelloWorld` is the same label.*
*Note that spaces between identifer and `$` may not be allowed in the future.*

## Goto

*Note: the current implementation only allows backward `goto`s, since it does not require much*
*declared variables analysis. Forward `goto`s may be supported in the future.*
```
func f:
    i := 0
$label1:
    Print(i)
    i++
    goto $label1 if i < 10
```

### Labeled blocks and control flow

Loops and if statements may have labled on them with the following syntax.

```
condition := true // any valid boolean expression
condition2 := true // any valid boolean expression
if $label1 condition:
    nop
else if $label2 condition2:
    nop
else:
    nop

while $label3 condition:
    nop

do $label4:
    nop
while condition

for $label5 i in 1..10:
    nop
```

`goto $labelOnLoop` will transfer control before the loop or if statement is executed.
In terms of `goto` locations, this is where the actual locations are mapped:
```
condition := true // any valid boolean expression
condition2 := true // any valid boolean expression

deflb $label1
if condition:
    nop
else:
    deflb $label2
    if condition2:
        nop
    else:
        nop

deflb $label3
while condition:
    nop

deflb $label4
do:
    nop
while condition

deflb $label5
for i in 1..10:
    nop
```

Note that these are not a direct equivalent, see the next section below for `break`, `continue`, and `exit`.

## Labeled control flow

`break`, `continue`, and `exit` may also indicate labels to mark which control flow or loop it `break`s, `continue`s, or `exit`s from.

For example,

```
// Concept Code: Not fully supported yet
for $outter i in 0..5:
    l := array[i, i+1, i+2, i+3, i+4]
    for j in 0..5:
        continue $outter if l[j] > 4
        Print(i * 10 + j)
    // this line will be skipped if the continue statement earlier hits
    Print($"All elements in l while i == {i} is greater than 4")


k := 0
if $outter k > 10:
    for i in 0..10:
        for j in 0..10:
            exit if someCondition(i, j, k)
    Print("All Conditions Satistifed")
```

## Functions

Functions are defined with `func` keyword with the following syntax:

```
// defines a function funcName without any arguments and return type
func funcName:
    // body
    Print("Hello!")
    Print("How are you doing?")

// the brackets are optional if the function does not accept any parameters
func funcName2():
    // body
    Print("Hello!")
    Print("How are you doing?")

i := 0
// defines a function funcName without any arguments but with return type
func count -> int:
    return i++


// defines a function funcName with 1 argument of type int, returning an integer
func double(a : int) -> int:
    return a *

// when a parameter does not have a type specified, it will use the type of the next one
// In this case, "a" has type of int.
// Last parameter must have a type specified.
func sum(a, b : int) -> int:
    return a + b

// "a" and "b" has type int, printValue has type bool
func sum2(a, b : int, printValue : bool) -> int:
    tmp := a + b
    if printValue:
        Print(tmp)
    return tmp

// same here, "a" and "b" has type int, printValue has type bool
func sum3(printValue : bool, a, b : int) -> int:
    tmp := a + b
    if printValue:
        Print(tmp)
    return tmp
```

In either a top level statement, *or inside another function body\**, a child function can be created.
Child function can access any symbols declared before the function is declared.

*\*Not yet implemented*

## Built in types and functions

As QuickCode is currently implemented to generate MSIL, it will map types to MSIL types.
Currently, the built in types and functions are as follows,
```
int - maps to System.Int32
bool - maps to System.Boolean

Print() - a function, maps to Console.WriteLine()
```

## String

*Note: Currently, QuickCode, as well as, QuickCode strings only supported ASCII characters. Any unicode characters in the program may lead to undefined behavior.*

```
"Strings!"
```

### String Interpolation

*Note: String Interpolation is currently not supported.*

```
i := 0
Print($"i is equal to {0}")
```

## Classes

*Classes, and structs are currently not yet implemented. The specs is also subject to change.*

Classes exists in the namespace. Classes can contain functions, fields, and properties. As noted above, these are subject to change.

Accessibility can be declared via built-in attributes. It is designed this way to keep the function declaration clear.

```
namespace QuickCode.Example:
    class SampleClass:
        field fieldName : int = 0

        // constructor is a reserved identifier inside a class
        func constructor:
            nop

        @public @static
        func CallMe():
            Print("Hello World")

        // Operator overloading is automatically static
        @public
        func +(left, right : SomeClass) -> SomeClass:
            ...
```