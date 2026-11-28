import { describe, expect, it } from 'vitest'
import { cutExcerpt, ExcerptError } from './excerpt'

const source = `namespace Demo;

public static class Maths
{
    /// <summary>Adds.</summary>
    [Pure]
    public static int Add(int a, int b) =>
        a + b;

    public static int Abs(int a)
    {
        if (a < 0) { return -a; }
        return a;
    }
}
`

const file = 'Maths.cs'

describe('cutExcerpt', () => {
  it('takes an expression-bodied member up to its semicolon, with its docs and attributes, dedented', () => {
    const cut = cutExcerpt(source, { file, from: 'public static int Add' })
    expect(cut.code).toBe('/// <summary>Adds.</summary>\n[Pure]\npublic static int Add(int a, int b) =>\n    a + b;')
    expect([cut.startLine, cut.endLine]).toEqual([5, 8])
  })

  it('takes a block up to its matching brace', () => {
    const cut = cutExcerpt(source, { file, from: 'public static int Abs' })
    expect(cut.code.split('\n').at(-1)).toBe('}')
    expect([cut.startLine, cut.endLine]).toEqual([10, 14])
  })

  it('can leave the docs out, and end at a given line', () => {
    const cut = cutExcerpt(source, { file, from: 'public static int Add', to: 'a + b', withoutDocs: true })
    expect(cut.startLine).toBe(7)
  })

  it('numbers highlighted lines within the excerpt', () => {
    const cut = cutExcerpt(source, { file, from: 'public static int Abs', highlight: ['return -a'] })
    expect([...cut.highlighted]).toEqual([3])
  })

  it.each([
    { file, from: 'Subtract' },
    { file, from: 'Add', to: 'Nowhere' },
    { file, from: 'public static int Abs', highlight: ['Missing'] },
  ])('fails loudly when the code it quotes has gone: %o', (spec) => {
    expect(() => cutExcerpt(source, spec)).toThrow(ExcerptError)
  })
})
