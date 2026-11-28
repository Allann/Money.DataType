import { createHighlighterCore, type HighlighterCore } from 'shiki/core'
import { createJavaScriptRegexEngine } from 'shiki/engine/javascript'

let highlighter: Promise<HighlighterCore> | undefined

/** One shared highlighter, loaded on first use with just the C# grammar and two themes. */
export function loadHighlighter(): Promise<HighlighterCore> {
  highlighter ??= createHighlighterCore({
    themes: [import('shiki/themes/github-light.mjs'), import('shiki/themes/github-dark.mjs')],
    langs: [import('shiki/langs/csharp.mjs')],
    engine: createJavaScriptRegexEngine(),
  })
  return highlighter
}

/** C# as highlighted HTML that follows the page's light or dark scheme, with `highlighted` lines marked. */
export function toHtml(shiki: HighlighterCore, code: string, highlighted: ReadonlySet<number>): string {
  return shiki.codeToHtml(code, {
    lang: 'csharp',
    themes: { light: 'github-light', dark: 'github-dark' },
    defaultColor: 'light-dark()',
    transformers: [
      {
        line(node, line) {
          if (highlighted.has(line)) this.addClassToHast(node, 'highlighted')
        },
      },
    ],
  })
}
