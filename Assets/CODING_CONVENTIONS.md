# C# Coding Standards

Source: https://docs.popekim.com/en/coding-standards/csharp

This is a project-local, indexed rewrite of the referenced standard. It keeps the same overall structure and rule order for easy lookup.

## Preface

### Rule Of Thumb

1. Put readability first.
2. Prefer the IDE's automatic formatting unless there is a strong reason to override it.
3. Follow the style already present in the surrounding code.

### References

1. Unreal Engine coding standards.
2. Doom 3 code style conventions.
3. IDesign C# coding standard.

### IDE Helper

1. Use the IDE settings linked from the source document when possible.

## I. Main Coding Standards

1. Use PascalCase for classes and structs.
2. Use camelCase for local variables and method parameters.
3. Prefer verb-object method names by default, such as `GetAge`.
4. Boolean state methods should usually start with `Is`, `Can`, `Has`, or `Should`; use a natural third-person verb if that reads better.
5. Use PascalCase for public method names.
6. Use camelCase for non-public method names.
7. Use `ALL_CAPS_SEPARATED_BY_UNDERSCORE` for constants.
8. Use `static readonly` when an object represents a constant value.
9. Use `ALL_CAPS_SEPARATED_BY_UNDERSCORE` for `static readonly` fields that behave like constants.
10. Use `readonly` when a variable should only be assigned once.
11. Use PascalCase for namespaces.
12. Prefix boolean variables with `b`; use `mb` for private boolean member variables.
13. Prefix boolean properties with `Is`, `Can`, `Should`, or `Has`.
14. Prefix interface names with `I`.
15. Prefix enum names with `E`.
16. Prefix struct names with `S`, except for `readonly struct`.
17. Prefix private member variables with `m`, then use PascalCase for the rest of the name.
18. Name methods with return values after the value they return.
19. Prefer descriptive variable names. Use very short names only for trivial loop indices.
20. Capitalize acronyms fully only when no normal word follows them; otherwise use normal PascalCase acronym casing.
21. Prefer properties over getter and setter methods.
22. Declare local variables near their first use.
23. Specify floating-point precision explicitly unless `double` is intended.
24. Always include a `default` case in `switch` statements.
25. If a `switch` `default` should be unreachable, fail loudly with `Debug.Fail()`.
26. Use `Debug.Assert()` for assumptions made while writing code.
27. End recursive method names with `Recursive`.
28. Order class contents as member variables, properties, constructors, then methods; order methods from public to private.
29. Group related member variables and methods together.
30. Avoid overloads when parameter types are too general; use explicit names instead.
31. Keep each class in its own file unless grouping small related classes is clearer.
32. Match the source filename to the class name, including casing.
33. For partial classes, name each file with the class name plus a dot and subsection name.
34. Use assertions for unrecoverable assumptions.
35. End bitflag enum names with `Flags`.
36. Prefer overloading over default parameters.
37. If default parameters are used, limit them to natural immutable values such as `null`, `false`, or `0`.
38. Do not shadow variables.
39. Prefer generic containers from `System.Collections.Generic`; arrays are fine when they are the natural choice.
40. Prefer explicit types over `var` unless the type is unimportant, such as anonymous types or some `IEnumerable` cases.
41. Use static classes instead of singleton patterns.
42. Use `async Task` instead of `async void`, except for event handlers.
43. Do not add an `Async` suffix to async method names.
44. Validate external data at the boundary, then assume internal data is valid.
45. Do not throw exceptions from non-boundary methods; handle exceptions at boundaries.
46. Exception throwing is allowed in `switch` defaults used to catch missing enum handling; do not catch those exceptions.
47. Prefer not to allow `null` parameters, especially in public methods.
48. If a parameter can be `null`, suffix the name with `OrNull`.
49. Prefer not to return `null`, especially from public methods, unless it avoids throwing.
50. If a method can return `null`, suffix the method name with `OrNull`.
51. Use inline lambdas only for simple single statements.
52. Avoid object initializers unless they are used with `required` and init-only setters.
53. Declare `out` variables on a separate line before the call.
54. Do not use the null-coalescing operator.
55. Do not use `using` declarations; use `using` statements.
56. Specify the type after `new`, except for anonymous types inside functions.
57. Use `private init` setters wherever possible.
58. Use file-scoped namespace declarations.
59. Use `readonly record struct` when strong-typing a generic type.

## II. Code Formatting

1. Use Visual Studio's default tab behavior. If another IDE is used, use 4 spaces instead of real tab characters.
2. Put opening braces on a new line.
3. Always use braces, even for one-line scopes.
4. Declare one variable per line.

## III. Project Settings

1. Treat compiler warnings as errors in release builds.
2. Do not use implicit global usings.

## IV. Framework Specific Guidelines

### A. Auto Serialization/Deserialization

1. Define auto-serializable data as classes.
2. Do not put serializer-specific attributes into auto-serializable classes.
3. Expose auto-serializable data through public auto-properties.
4. Use public methods instead of read-only properties when calculated read-only data is needed.
5. Auto-serializable classes should have only one public parameterless constructor.
6. Wrap serializer API calls instead of calling them directly throughout the codebase.

### B. XAML Controls

1. Do not name a control unless a name is truly needed.
2. Use PascalCase with an `x` prefix for control names.
3. Prefix control names with the full control type.

### C. ASP.NET Core

1. For REST request DTO bodies, make value-type properties nullable so model validation can catch missing values.
2. Validate controller inputs at the start of the method; after validation, assume inputs are valid.
3. Route parameters should not use nullable value types just because request-body DTO properties do.
