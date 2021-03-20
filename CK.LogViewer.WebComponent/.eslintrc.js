module.exports = {
    root: true,
    parser: '@typescript-eslint/parser',
    plugins: [
        '@typescript-eslint',
    ],
    extends: [
        'eslint:recommended',
        'plugin:@typescript-eslint/recommended',
    ],
    env: {
        node: true,
        browser: true,
        es6: true
    },
    rules: {
        "@typescript-eslint/no-explicit-any": 0,
        "semi": "off",
        "@typescript-eslint/semi": ["error"],
        "no-debugger": 0
    }

};
