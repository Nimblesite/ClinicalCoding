import js from '@eslint/js';
import type { Linter } from 'eslint';
import eslintComments from 'eslint-plugin-eslint-comments';
import functional from 'eslint-plugin-functional';
import importX from 'eslint-plugin-import-x';
import jsxA11y from 'eslint-plugin-jsx-a11y';
import noSecrets from 'eslint-plugin-no-secrets';
import perfectionist from 'eslint-plugin-perfectionist';
import playwright from 'eslint-plugin-playwright';
import promise from 'eslint-plugin-promise';
import react from 'eslint-plugin-react';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import regexp from 'eslint-plugin-regexp';
import security from 'eslint-plugin-security';
import sonarjs from 'eslint-plugin-sonarjs';
import tsdoc from 'eslint-plugin-tsdoc';
import unicorn from 'eslint-plugin-unicorn';
import vitest from 'eslint-plugin-vitest';
import tseslint from 'typescript-eslint';

const config: Linter.Config[] = [
  {
    ignores: ['dist', 'node_modules', 'coverage', 'playwright-report', 'test-results'],
  },
  // Base JS/TS configuration
  js.configs.recommended,
  ...tseslint.configs.strictTypeChecked,
  ...tseslint.configs.stylisticTypeChecked,
  {
    languageOptions: {
      parserOptions: {
        project: './tsconfig.json',
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
  // React configuration
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      react,
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
    },
    languageOptions: {
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
        },
      },
    },
    settings: {
      react: {
        version: 'detect',
      },
    },
    rules: {
      ...react.configs.recommended.rules,
      ...react.configs['jsx-runtime'].rules,
      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': 'error',
      'react/jsx-no-leaked-render': 'error',
      'react/no-array-index-key': 'error',
      'react/no-unstable-nested-components': 'error',
      'react/jsx-key': ['error', { checkFragmentShorthand: true, warnOnDuplicates: true }],
      'react/function-component-definition': ['error', { namedComponents: 'arrow-function' }],
      'react-hooks/exhaustive-deps': 'error',
    },
  },
  // JSX A11y
  {
    files: ['**/*.{jsx,tsx}'],
    plugins: {
      'jsx-a11y': jsxA11y,
    },
    rules: jsxA11y.configs.strict.rules,
  },
  // Import rules
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      'import-x': importX,
    },
    rules: {
      'import-x/no-cycle': 'off',
      'import-x/no-self-import': 'off',
      'import-x/no-useless-path-segments': 'off',
      'import-x/no-default-export': 'off',
      'import-x/no-extraneous-dependencies': 'off',
    },
  },
  // Unicorn
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      unicorn,
    },
    rules: {
      ...unicorn.configs.recommended.rules,
      'unicorn/filename-case': 'off',
      'unicorn/prefer-node-protocol': 'error',
      'unicorn/no-array-for-each': 'off',
      'unicorn/prefer-module': 'error',
      'unicorn/numeric-separators-style': 'off',
      'unicorn/prevent-abbreviations': 'off',
      'unicorn/no-null': 'off',
      'unicorn/no-negated-condition': 'off',
      'unicorn/catch-error-name': 'off',
      'unicorn/prefer-code-point': 'off',
      'unicorn/prefer-response-static-json': 'off',
    },
  },
  // Promise
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      promise,
    },
    rules: {
      'promise/catch-or-return': 'error',
      'promise/no-return-wrap': 'error',
      'promise/param-names': 'error',
      'promise/no-nesting': 'warn',
    },
  },
  // SonarJS
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      sonarjs,
    },
    rules: {
      'sonarjs/cognitive-complexity': ['error', 15],
      'sonarjs/no-duplicate-string': ['error', { threshold: 3 }],
      'sonarjs/no-identical-functions': 'error',
      'sonarjs/no-collapsible-if': 'error',
      'sonarjs/no-redundant-jump': 'error',
    },
  },
  // No secrets
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      'no-secrets': noSecrets,
    },
    rules: {
      'no-secrets/no-secrets': 'error',
    },
  },
  // ESLint comments
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      'eslint-comments': eslintComments,
    },
    rules: {
      'eslint-comments/no-unused-disable': 'error',
      'eslint-comments/require-description': 'error',
    },
  },
  // Functional
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      functional,
    },
    rules: {
      'functional/no-let': 'off',
      'functional/immutable-data': 'off',
      'functional/no-loop-statements': 'off',
    },
  },
  // Regexp
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      regexp,
    },
    rules: {
      'regexp/no-super-linear-backtracking': 'error',
      'regexp/no-empty-alternative': 'error',
    },
  },
  // Security
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      security,
    },
    rules: {
      'security/detect-eval-with-expression': 'error',
      'security/detect-non-literal-fs-filename': 'error',
      'security/detect-unsafe-regex': 'error',
    },
  },
  // Perfectionist
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      perfectionist,
    },
    rules: {
      'perfectionist/sort-imports': 'off',
      'perfectionist/sort-named-imports': 'off',
      'perfectionist/sort-jsx-props': 'off',
    },
  },
  // TSDoc
  {
    files: ['**/*.{ts,tsx}'],
    plugins: {
      tsdoc,
    },
    rules: {
      'tsdoc/syntax': 'error',
    },
  },
  // TypeScript-specific rules
  {
    files: ['**/*.{ts,tsx}'],
    rules: {
      '@typescript-eslint/no-unsafe-call': 'error',
      '@typescript-eslint/no-unsafe-member-access': 'error',
      '@typescript-eslint/no-unsafe-argument': 'error',
      '@typescript-eslint/no-unsafe-assignment': 'error',
      '@typescript-eslint/no-unsafe-return': 'error',
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-non-null-assertion': 'error',
      '@typescript-eslint/strict-boolean-expressions': [
        'error',
        {
          allowString: false,
          allowNumber: false,
          allowNullableObject: false,
          allowNullableBoolean: false,
          allowNullableString: false,
          allowNullableNumber: false,
          allowAny: false,
          allowRuleToRunWithoutStrictNullChecksIKnowWhatIAmDoing: false,
        },
      ],
      '@typescript-eslint/switch-exhaustiveness-check': 'error',
      '@typescript-eslint/no-floating-promises': 'error',
      '@typescript-eslint/no-misused-promises': 'error',
      '@typescript-eslint/explicit-function-return-type': 'error',
      '@typescript-eslint/explicit-member-accessibility': 'error',
      '@typescript-eslint/no-shadow': 'error',
      '@typescript-eslint/consistent-type-imports': ['error', { fixStyle: 'inline-type-imports' }],
      '@typescript-eslint/no-require-imports': 'error',
      '@typescript-eslint/prefer-readonly': 'error',
      '@typescript-eslint/require-array-sort-compare': 'error',
      '@typescript-eslint/promise-function-async': 'error',
      '@typescript-eslint/no-deprecated': 'error',
      '@typescript-eslint/no-unused-vars': [
        'error',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_' },
      ],
      '@typescript-eslint/consistent-type-definitions': ['error', 'interface'],
      '@typescript-eslint/no-unnecessary-condition': 'error',
      '@typescript-eslint/no-unnecessary-type-arguments': 'error',
      '@typescript-eslint/no-unnecessary-type-assertion': 'error',
      '@typescript-eslint/no-unnecessary-boolean-literal-compare': 'error',
      '@typescript-eslint/prefer-nullish-coalescing': 'error',
      '@typescript-eslint/prefer-optional-chain': 'error',
      '@typescript-eslint/prefer-reduce-type-parameter': 'error',
      '@typescript-eslint/prefer-ts-expect-error': 'error',
      '@typescript-eslint/no-base-to-string': 'error',
      '@typescript-eslint/restrict-plus-operands': 'error',
      '@typescript-eslint/restrict-template-expressions': 'error',
      '@typescript-eslint/no-confusing-void-expression': 'error',
      '@typescript-eslint/no-meaningless-void-operator': 'error',
      '@typescript-eslint/no-redundant-type-constituents': 'error',
      '@typescript-eslint/no-useless-empty-export': 'error',
      '@typescript-eslint/method-signature-style': ['error', 'property'],
      '@typescript-eslint/array-type': ['error', { default: 'array-simple' }],
      '@typescript-eslint/no-magic-numbers': 'off',
    },
  },
  // General rules
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    rules: {
      eqeqeq: ['error', 'always'],
      'no-param-reassign': 'error',
      'prefer-const': 'error',
      'no-console': ['error', { allow: ['warn', 'error'] }],
      'no-debugger': 'error',
      'no-alert': 'error',
      'prefer-template': 'error',
    },
  },
  // Override for files that need default exports (Vite entry points, route components)
  {
    files: ['src/main.tsx', 'src/App.tsx'],
    rules: {
      'import-x/no-default-export': 'off',
    },
  },
  // Vitest test files
  {
    files: ['src/**/*.test.ts', 'src/**/*.test.tsx', 'tests/**/*'],
    plugins: {
      vitest,
    },
    rules: {
      ...vitest.configs.recommended.rules,
      '@typescript-eslint/no-non-null-assertion': 'off',
      '@typescript-eslint/no-unsafe-call': 'off',
      '@typescript-eslint/no-unsafe-member-access': 'off',
      '@typescript-eslint/no-unsafe-argument': 'off',
      '@typescript-eslint/no-unsafe-assignment': 'off',
      '@typescript-eslint/no-unsafe-return': 'off',
      '@typescript-eslint/no-explicit-any': 'off',
      '@typescript-eslint/explicit-function-return-type': 'off',
      '@typescript-eslint/strict-boolean-expressions': 'off',
      '@typescript-eslint/no-unnecessary-condition': 'off',
      'functional/no-let': 'off',
      'sonarjs/no-duplicate-string': 'off',
    },
  },
  // Playwright E2E tests
  {
    files: ['e2e/**/*.spec.ts', 'e2e/**/*.test.ts'],
    plugins: {
      playwright,
    },
    rules: {
      ...playwright.configs.recommended.rules,
      '@typescript-eslint/no-non-null-assertion': 'off',
      '@typescript-eslint/no-unsafe-call': 'off',
      '@typescript-eslint/no-unsafe-member-access': 'off',
      '@typescript-eslint/no-unsafe-argument': 'off',
      '@typescript-eslint/no-unsafe-assignment': 'off',
      '@typescript-eslint/no-unsafe-return': 'off',
      '@typescript-eslint/no-explicit-any': 'off',
      '@typescript-eslint/explicit-function-return-type': 'off',
      '@typescript-eslint/strict-boolean-expressions': 'off',
      '@typescript-eslint/no-unnecessary-condition': 'off',
      'functional/no-let': 'off',
      'sonarjs/no-duplicate-string': 'off',
    },
  },
];

export default config;
