function woopAdapter(properties, methods) {
    var input = {
        fullText: properties.fullText,
        selection: properties.selection,
        isSelection: properties.isSelection,
        postInfo: methods.postInfo,
        postError: methods.postError,
        insert: methods.insert,

        get text() {
            return this.isSelection ? this.selection : this.fullText;
        },

        set text(value) {
            if (this.isSelection) {
                this.selection = value;
            } else {
                this.fullText = value;
            }
        }
    };
    main(input);

    return input.text;
}