const path = require('path');
var glob = require("glob");
const ESLintPlugin = require('eslint-webpack-plugin');

const CopyPlugin = require("copy-webpack-plugin");
module.exports = {
    entry: glob.sync("./src/components/**/*.ts"),
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
                include: [path.resolve(__dirname, 'src')]
            },
            {
                test: /web-components\//,
                loader: 'web-components-loader'
            }
        ]
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    output: {
        publicPath: '',
        filename: 'bundle.js',
        path: path.resolve('../CK.LogViewer.WebApp/wwwroot')
    },
    plugins: [
        new CopyPlugin({
            patterns: [
                {
                    from: "node_modules/@webcomponents/custom-elements/",
                    to: "node_modules/@webcomponents/custom-elements/"
                },
                {
                    from: "src/index.html",
                    to: "index.html"
                },
                {
                    from: "src/styles.css",
                    to: "styles.css"
                }]
        }),
        new ESLintPlugin({
            context: "src",
            extensions: "ts"
        })
    ]
};
