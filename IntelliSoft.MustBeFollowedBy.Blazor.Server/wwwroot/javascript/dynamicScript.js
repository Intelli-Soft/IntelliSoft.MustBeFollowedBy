window.executeDynamicScript = function (scriptContent) {
    try {
        eval(scriptContent);
    } catch (e) {
        console.error('Error on running dynamic Script', e);
    }
};


