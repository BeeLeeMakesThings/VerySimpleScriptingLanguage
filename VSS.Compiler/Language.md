# Language Usage

## Hello World

```
write("Hello world");
```

## Variables

```
int x = 10;
x = x + 1;
print("X=" + x);
```

## User Input

```
string input = read();
print("User typed " + input);
```

## Branching

```
int x;
x=10;
if(x > 10)
    print("x is greater than 10");
else 
    print("x is at most 10");
```

## Looping

```
int x = 0;
while(x<10) {
    print("x=" + x);
    x = x + 1;
}
```

# Grammar

```

LHS := id |     # x
       id id    # int x

RHS := id |       # x
       ( RHS ) |  # (x)
       


# Abbreviations/Terminals
# id: identifier
# eps: empty string

```

# Grammar - Cleaned up and Factorised

```

LHS := id LHS2
LHS2 := id | eps
```