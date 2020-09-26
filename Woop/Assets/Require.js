function require(name) {
    const module = { exports: {} };
    ((module, exports) => {
        eval(_loadLib(name))
    })(module, module.exports);
    return module.exports;
};