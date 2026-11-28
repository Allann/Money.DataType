# Architecture decision records

Short records of the decisions that shape this library: what was decided, why, and what it costs.
Each one is a few paragraphs, in the order the decisions were made.

| # | Decision |
| --- | --- |
| [0001](0001-bankers-rounding-by-default.md) | Round amounts with banker's rounding by default |
| [0002](0002-explicit-minor-units.md) | Model a currency's minor unit explicitly |
| [0003](0003-immutable-value-records.md) | Money types are immutable value records, serialised with System.Text.Json only |
| [0004](0004-a-process-wide-currency-registry.md) | Keep a process-wide, thread-safe currency registry |
| [0005](0005-language-grammar-behind-ilanguage.md) | Number words: each language owns its grammar |
