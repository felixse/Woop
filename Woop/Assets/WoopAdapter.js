function woopAdapter(properties, methods) {
    var input = {
        text: properties.selection && properties.selection != "" ? properties.selection : properties.fullText,
        fullText: properties.fullText,
        selection: properties.selection,
        isSelection: properties.isSelection,
        postInfo: methods.postInfo,
        postError: methods.postError,
        insert: methods.insert
    };
    main(input);

    return input.text;
}