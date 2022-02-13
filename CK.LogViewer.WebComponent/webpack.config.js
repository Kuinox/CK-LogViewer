const path = require('path');
var glob = require("glob");
const ESLintPlugin = require('eslint-webpack-plugin');
const CopyPlugin = require("copy-webpack-plugin");
const ReplaceInFileWebpackPlugin = require('replace-in-file-webpack-plugin');
const packageJson = require('./package.json');
const { ProvidePlugin } = require('webpack');
const outputPath = path.resolve('../CK.LogViewer.WebApp/wwwroot');
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
        fallback: {
            Buffer: require.resolve('buffer/'),
            process: require.resolve('process/'),
        }
    },
    output: {
        publicPath: '',
        filename: 'bundle.js',
        path: outputPath
    },
    plugins: [
        new CopyPlugin({
            patterns: [
                {
                    from: "node_modules/@webcomponents/custom-elements/src/native-shim.js",
                    to: "native-shim.js"
                },
                {
                    from: "src/index.html",
                    to: "index.html"
                },
                {
                    from: "src/styles.css",
                    to: "styles.css"
                },
                {
                    from: "src/Inconsolata.woff2",
                    to: "Inconsolata.woff2"
                }
            ]
        }),
        new ReplaceInFileWebpackPlugin([{
            files: [`${outputPath}/index.html`],
            rules: [{
                search: /\$VERSION/gi,
                replace: packageJson.version
            }]
        }]),
        new ESLintPlugin({
            context: "src",
            extensions: "ts"
        }),
        new ProvidePlugin({ process: ['process'], Buffer: ['buffer', 'Buffer'] })
    ]
};
