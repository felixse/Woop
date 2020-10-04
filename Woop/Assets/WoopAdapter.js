function woopAdapter(properties, methods) {
    var input = {
        fullText: properties.fullText,
        selection: properties.selection,
        isSelection: properties.isSelection,
        insertPosition: properties.insertPosition,
        postInfo: methods.postInfo,
        postError: methods.postError,

        get text() {
            return this.isSelection ? this.selection : this.fullText;
        },

        set text(value) {
            if (this.isSelection) {
                this.selection = value;
            } else {
                this.fullText = value;
            }
        },

        insert: (value) => {
            if (input.isSelection) {
                input.text = value;
                return;
            }

            if (!input.text) {
                input.text = value;
                return;
            }

            var newText = input.text.slice(0, input.insertPosition) + value + input.text.slice(input.insertPosition);
            input.text = newText;
        }
    };

    main(input);

    return input.text;
}