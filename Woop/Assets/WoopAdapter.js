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

    // todo this breaks if a script is using fullText or selection instead of text
    return input.text;
}