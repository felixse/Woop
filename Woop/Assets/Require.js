function require(name) {
    const module = { exports: {} };
    ((module, exports) => {
        eval(_loader.Load(name))
    })(module, module.exports);
    return module.exports;
};