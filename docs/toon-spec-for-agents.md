# TOON Format Specification for AI Agents

## Purpose
TOON (Token-Oriented Object Notation) is a deterministic, token-efficient format for structured data transmission to Large Language Models. It reduces token usage by 30-60% compared to JSON while maintaining readability.

## Core Principles
1. **Deterministic**: One input produces exactly one output
2. **Minimal**: No redundant syntax (no closing braces, minimal quotes)
3. **Self-documenting**: Array lengths and field names explicitly declared
4. **Delimiter-aware**: Supports comma (default), tab, or pipe delimiters

## Encoding Rules

### Primitives

#### Numbers
- Emit in decimal form without scientific notation
- Convert `-0` to `0`
- Convert `NaN`, `Infinity`, `-Infinity` to `null`
- BigInt: emit as decimal digits without quotes

#### Booleans
- Emit as `true` or `false`

#### Null
- Emit as `null`

#### Strings
Quote strings ONLY when they:
1. Are empty (`""`)
2. Have leading/trailing whitespace (`" padded "`)
3. Contain the active delimiter or colon (`"a,b"` for comma, `"a|b"` for pipe)
4. Contain quotes, backslash, or control characters (`"say \"hi\""`, `"C:\\path"`, `"line\nbreak"`)
5. Look like boolean/number/null (`"true"`, `"42"`, `"-3.14"`, `"null"`)
6. Start with `"- "` (list marker)
7. Look like structural tokens (`"[5]"`, `"{key}"`)

Escape within quoted strings:
- `\"` for quotes
- `\\` for backslash
- `\n`, `\r`, `\t` for control characters

### Objects

#### Empty Object
- Emit nothing (empty string)

#### Simple Objects
```
key1: value1
key2: value2
```
- One space after colon for primitives
- No space after colon for nested/empty objects

#### Nested Objects
```
parent:
  child1: value1
  child2: value2
```
- Indent with 2 spaces per level

#### Object Keys
Quote keys when they:
1. Contain non-identifier characters (spaces, hyphens, colons, delimiters, brackets, braces)
2. Start with a digit
3. Are empty string
4. Contain control characters

Valid unquoted keys match: `[a-zA-Z_][a-zA-Z0-9_.]*`

### Arrays

#### Array Header Format
- Comma delimiter (implicit): `[N]` or `[N]{fields}`
- Tab delimiter (explicit): `[N	]` or `[N	]{field1	field2}`
- Pipe delimiter (explicit): `[N|]` or `[N|]{field1|field2}`
- Optional length marker: `[#N]` prefix

#### Empty Arrays
```
items[0]:
```

#### Primitive Arrays (Inline)
```
tags[3]: reading,gaming,coding
```
- Values on same line as header
- Quote values containing delimiter/colon/special chars

#### Tabular Arrays (Uniform Objects)
Requirements for tabular format:
1. All elements are objects
2. All objects have identical keys
3. All values are primitives
4. Key order from first object

Format:
```
items[2]{sku,qty,price}:
  A1,2,9.99
  B2,1,14.5
```
- Field names in header use active delimiter
- One row per line, indented 2 spaces
- Values separated by active delimiter

#### List Arrays (Non-uniform)
When tabular requirements not met:
```
items[3]:
  - 1
  - name: Ada
  - text
```
- Each item starts with `"  - "` (2 spaces, hyphen, space)
- Object fields indented under hyphen line

#### Nested Arrays
Arrays containing arrays:
```
pairs[2]:
  - [2]: 1,2
  - [2]: 3,4
```

#### Root Arrays
At document root:
```
[3]: a,b,c
```

### Special Cases

#### First Field Placement in List Objects
When first field is non-primitive:
```
items[1]:
  - users[2]{id,name}:
    1,Ada
    2,Bob
    status: active
```
Array contents indented under header, subsequent fields at same level.

#### Type Conversions
- `Date` → ISO string in quotes
- `undefined` → `null`
- `function` → `null`
- `symbol` → `null`

### Formatting Invariants
1. No trailing spaces on any line
2. No trailing newline at document end
3. Exactly 2 spaces per indentation level
4. Single space after colon for primitives
5. No space after colon for nested/empty structures

## Implementation Checklist

### Parser Requirements
1. Split key-value on first `": "` (space is required for primitives)
2. Track indentation level (2 spaces = 1 level)
3. Detect array headers by `[N` pattern
4. Parse delimiter from header for non-comma delimiters
5. Handle quoted strings with escape sequences
6. Validate array lengths match declared count

### Encoder Requirements
1. Analyze arrays for tabular eligibility before encoding
2. Apply quoting rules based on active delimiter
3. Maintain deterministic key ordering
4. Convert non-JSON types appropriately
5. Respect formatting invariants

## Error Conditions
1. Array length mismatch (declared vs actual)
2. Inconsistent indentation
3. Missing colon-space separator for primitives
4. Unescaped quotes in quoted strings
5. Mismatched field count in tabular rows

## Optimization Guidelines
1. Use tab delimiter for datasets with many commas
2. Use pipe delimiter for readability in documentation
3. Prefer tabular format by ensuring uniform object structure
4. Minimize quoting by choosing appropriate delimiter
5. Consider length marker (#) for clarity in generated output