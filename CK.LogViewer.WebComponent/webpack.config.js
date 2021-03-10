const path = require('path');
var glob = require("glob");
const CopyPlugin = require("copy-webpack-plugin");

module.exports = {
    entry: glob.sync("./src/components/*.ts"),
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
        publicPath: 'public',
        filename: 'bundle.js',
        path: path.resolve(__dirname, 'public'),
    },
    plugins: [
        new CopyPlugin({
            patterns: [
                {
                    from: "node_modules/@webcomponents/custom-elements/",
                    to: "node_modules/@webcomponents/custom-elements/"
                },
                {
                    from: "index.html",
                    to: "index.html"
                }]
        })
    ]
};
