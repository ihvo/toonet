# Token-Oriented Object Notation (TOON) Specification

Version 1.0

## Abstract

Token-Oriented Object Notation (TOON) is a compact, human-readable data serialization format optimized for Large Language Model (LLM) input. TOON achieves 30-60% token reduction compared to JSON by eliminating redundant syntax while maintaining parseability through explicit structural markers and indentation-based nesting.

## 1. Introduction

### 1.1 Design Goals

1. **Token Efficiency**: Minimize token usage for LLM processing
2. **Human Readability**: Maintain clarity without redundant punctuation
3. **Deterministic Output**: One input produces exactly one canonical output
4. **Self-Documenting**: Include explicit counts and field declarations
5. **Parse Simplicity**: Enable parsing with simple string operations

### 1.2 Key Innovations

- **Tabular Arrays**: Declare fields once, stream rows without repetition
- **Indentation Structure**: Replace braces with meaningful whitespace
- **Explicit Lengths**: Array headers include element count `[N]`
- **Minimal Quoting**: Quote only when ambiguous or necessary
- **Delimiter Flexibility**: Support comma, tab, or pipe delimiters

## 2. Grammar

### 2.1 BNF Notation

```bnf
; Document root
document        ::= value

; Values
value           ::= primitive
                  | object
                  | array

; Primitives
primitive       ::= number
                  | boolean
                  | null
                  | string

number          ::= "-"? digit+ ("." digit+)?
boolean         ::= "true" | "false"
null            ::= "null"
string          ::= unquoted-string | quoted-string
unquoted-string ::= (safe-char)+
quoted-string   ::= '"' (escaped-char | non-quote-char)* '"'
escaped-char    ::= "\\" ("\"" | "\\" | "n" | "r" | "t")

; Objects
object          ::= ""
                  | object-content
object-content  ::= object-line (newline object-line)*
object-line     ::= indent key separator value
key             ::= identifier | quoted-string
identifier      ::= letter (letter | digit | "_" | ".")*
separator       ::= ": " | ":"

; Arrays
array           ::= array-header array-content?
array-header    ::= "[" length-marker? count delimiter-marker? "]" field-list? ":"
length-marker   ::= "#"
count           ::= digit+
delimiter-marker::= "" | "\t" | "|"
field-list      ::= "{" field-names "}"
field-names     ::= key (delimiter key)*

; Array content types
array-content   ::= inline-content
                  | tabular-content
                  | list-content

; Inline arrays (primitives only)
inline-content  ::= " " value (delimiter value)*

; Tabular arrays (uniform objects)
tabular-content ::= (newline indent row)+
row             ::= value (delimiter value)*

; List arrays (mixed/non-uniform)
list-content    ::= (newline indent "- " list-item)+
list-item       ::= value
                  | object-field (newline indent "  " object-field)*
object-field    ::= key separator value

; Utilities
delimiter       ::= "," | "\t" | "|"
indent          ::= "  "*
newline         ::= "\n"
letter          ::= "a".."z" | "A".."Z" | "_"
digit           ::= "0".."9"
safe-char       ::= <any char except delimiter, ":", quotes, controls, leading/trailing space>
```

### 2.2 Structural Rules

#### 2.2.1 Indentation
- Each nesting level adds exactly 2 spaces
- No tabs for indentation (tabs reserved as delimiter)
- Indentation determines structure hierarchy

#### 2.2.2 Array Length Declaration
- `[N]`: N is the exact count of elements
- `[#N]`: Optional hash prefix emphasizes count (not index)
- Parser MUST validate actual count matches declared count

#### 2.2.3 Delimiter Encoding
- Comma delimiter: Implicit in headers (`[N]`, `{f1,f2}`)
- Tab delimiter: Explicit (`[N	]`, `{f1	f2}`)
- Pipe delimiter: Explicit (`[N|]`, `{f1|f2}`)

## 3. Data Types

### 3.1 Primitives

#### 3.1.1 Numbers
```
42
-3.14
0.000001
9007199254740991
```
- No scientific notation in output
- Convert `-0` to `0`
- Non-finite values (`NaN`, `±Infinity`) become `null`
- BigInt emitted as decimal without quotes

#### 3.1.2 Booleans
```
true
false
```

#### 3.1.3 Null
```
null
```

#### 3.1.4 Strings

**Unquoted strings** allowed when:
- No leading/trailing whitespace
- No delimiter or colon characters
- Not ambiguous with other types
- No control characters

**Quoted strings** required when:
- Empty string: `""`
- Leading/trailing spaces: `" padded "`
- Contains delimiter or colon: `"a,b"`, `"key:value"`
- Contains quotes/backslash: `"say \"hello\""`, `"C:\\path"`
- Looks like other type: `"true"`, `"42"`, `"null"`
- Structural ambiguity: `"[5]"`, `"- item"`

### 3.2 Objects

#### 3.2.1 Empty Object
```
(empty output)
```

#### 3.2.2 Simple Object
```
id: 123
name: Ada
active: true
```

#### 3.2.3 Nested Object
```
user:
  id: 123
  profile:
    name: Ada
    tags[2]: reading,gaming
```

### 3.3 Arrays

#### 3.3.1 Empty Array
```
items[0]:
```

#### 3.3.2 Primitive Array (Inline)
```
tags[3]: reading,gaming,coding
nums[4]: 1,2,3,4
```

#### 3.3.3 Tabular Array (Uniform Objects)

Requirements:
- All elements are objects
- Identical key sets across all objects
- Primitive values only (no nested structures)

```
users[3]{id,name,role,active}:
  1,Alice,admin,true
  2,Bob,user,false
  3,Charlie,user,true
```

#### 3.3.4 List Array (Non-uniform)

Used when tabular requirements not met:

```
items[3]:
  - 1
  - name: Widget
    price: 9.99
  - simple text
```

#### 3.3.5 Nested Arrays

```
matrix[2]:
  - [2]: 1,2
  - [3]: 3,4,5
```

## 4. Encoding Rules

### 4.1 Object Key Quoting

Quote keys when they:
- Contain non-identifier characters (spaces, hyphens, colons, brackets)
- Start with a digit
- Are empty string
- Contain control characters

Valid unquoted keys: `/^[a-zA-Z_][a-zA-Z0-9_.]*$/`

### 4.2 String Value Quoting

| Condition | Example | Result |
|-----------|---------|--------|
| Empty | `""` | `""` |
| Whitespace edges | `" text "` | `" text "` |
| Contains delimiter | `a,b` | `"a,b"` |
| Contains colon | `key:val` | `"key:val"` |
| Looks like boolean | `true` | `"true"` |
| Looks like number | `42` | `"42"` |
| Starts with hyphen-space | `- item` | `"- item"` |
| Structural token | `[5]` | `"[5]"` |

### 4.3 Escape Sequences

Within quoted strings:
- `\"` → quote character
- `\\` → backslash
- `\n` → newline
- `\r` → carriage return
- `\t` → tab

### 4.4 Type Conversions

| JavaScript Type | TOON Output |
|----------------|-------------|
| `Date` | ISO string (quoted) |
| `undefined` | `null` |
| `function` | `null` |
| `symbol` | `null` |
| `BigInt` | Decimal digits |
| `NaN` | `null` |
| `±Infinity` | `null` |

## 5. Formatting Invariants

1. **No trailing spaces**: Lines never end with space characters
2. **No trailing newline**: Document never ends with newline
3. **Consistent indentation**: Exactly 2 spaces per level
4. **Separator spacing**:
   - Primitives: `key: value` (space required)
   - Nested/empty: `key:` (no space)
5. **Deterministic output**: Same input always produces identical output

## 6. Examples

### 6.1 User Profile
```
user:
  id: 123
  name: Ada Lovelace
  email: ada@example.com
  tags[3]: programming,mathematics,computing
  active: true
  metadata:
    created: "2025-01-15T10:00:00Z"
    lastLogin: "2025-01-20T14:30:00Z"
```

### 6.2 E-commerce Order
```
order:
  id: ORD-2025-001
  customer:
    name: John Doe
    email: john@example.com
  items[3]{sku,name,qty,price}:
    WDG-001,Widget,2,9.99
    GDG-002,Gadget,1,19.99
    THG-003,Thing,5,4.99
  total: 64.92
  status: shipped
```

### 6.3 Analytics Data (Tab-delimited)
```
metrics[5	]{date	views	clicks	revenue}:
  2025-01-01	5234	234	1205.50
  2025-01-02	6122	301	1455.75
  2025-01-03	4890	189	980.00
  2025-01-04	7234	412	2103.25
  2025-01-05	6543	356	1877.90
```

## 7. Implementation Notes

### 7.1 Parsing Strategy

1. **Line-based processing**: Split on newlines, track indentation
2. **Key-value separation**: Split on first `": "` or `":` at line end`
3. **Array detection**: Pattern match `[N` at start of value
4. **Delimiter extraction**: Parse from array header
5. **Quote handling**: State machine for quoted strings with escapes

### 7.2 Encoding Strategy

1. **Array analysis**: Pre-scan arrays for tabular eligibility
2. **Quote determination**: Check each string against quoting rules
3. **Indentation tracking**: Maintain current depth during traversal
4. **Buffer management**: Build output incrementally without backtracking

### 7.3 Performance Considerations

- **Token counting**: Varies by model tokenizer
- **Delimiter choice**: Tabs often tokenize better than commas
- **Tabular preference**: Uniform structures save significant tokens
- **Quote minimization**: Choose delimiter to reduce quoting needs

## 8. Security Considerations

1. **Length validation**: Verify declared array lengths to prevent buffer issues
2. **Escape handling**: Properly handle escape sequences to prevent injection
3. **Depth limits**: Consider maximum nesting depth for recursive parsers
4. **Size limits**: Set reasonable limits for string and array sizes

## 9. Future Extensions

Potential additions in future versions:
- Binary data encoding
- Schema declarations
- Compression directives
- Streaming support
- Comments

## Appendix A: Quick Reference Card

```
PRIMITIVES
  42, -3.14           Numbers
  true, false         Booleans
  null                Null
  hello, "he llo"     Strings

OBJECTS
  key: value          Simple field
  key:                Nested object
    child: value        (2-space indent)

ARRAYS
  [0]:                Empty
  [3]: a,b,c          Inline (primitives)
  [2]{id,name}:       Tabular (uniform objects)
    1,Alice
    2,Bob
  [2]:                List (non-uniform)
    - first
    - second

DELIMITERS
  ,                   Comma (default, implicit)
  [3	]: a	b	c      Tab (explicit in header)
  [3|]: a|b|c         Pipe (explicit in header)

QUOTING TRIGGERS
  ""                  Empty string
  " spaced "          Leading/trailing space
  "a,b"               Contains delimiter
  "key:value"         Contains colon
  "true", "42"        Ambiguous type
  "[5]", "- x"        Structural token
```

---

*This specification defines TOON version 1.0. For the latest updates and reference implementation, see [github.com/johannschopplich/toon](https://github.com/johannschopplich/toon)*
