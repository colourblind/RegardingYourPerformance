function DataFeed(targetDivId, feedUrl, feedName)
{
    this.feedUrl = feedUrl;
    this.feedName = feedName;
    this.feedId = this.feedName.replace(/ /g, '_');
    this.data = {
        cpu : [],
        memory : [],
        requests : []
    }
    this.numPoints = 40;
    
    this.fetchData = function() {
        $.jsonp({
            type : 'GET',
            url : this.feedUrl,
            callbackParameter: 'callback',
            dataType : 'jsonp',
            success : createDelegate(this, this.update),
            error : createDelegate(this, this.ruhroh)
        });
    }
    
    this.ruhroh = function(responseData)
    {
        var feedDiv = $('#' + this.feedId);
        feedDiv.addClass('fatal');
        feedDiv.toggleClass('flash');
        
        this.pushData(this.data.cpu, '0');
        this.pushData(this.data.memory, '0');
        this.pushData(this.data.requests, '0');

        this.draw();
    }
    
    this.update = function(responseData)
    {
        $('#' + this.feedId).removeClass('fatal');

        this.pushData(this.data.cpu, responseData.CpuUsage);
        this.pushData(this.data.memory, responseData.MemoryUsage);
        this.pushData(this.data.requests, responseData.RequestsPerSecond);
        
        this.draw();
    }
    
    this.draw = function()
    {
        $('#' + this.feedId + ' div.cpu').sparkline(this.data.cpu, { chartRangeMin : 0, chartRangeMax : 100, width : '100%', height : '40px', lineColor : '#f66', fillColor : '#faa' }); 
        $('#' + this.feedId + ' div.memory').sparkline(this.data.memory, { chartRangeMin : 0, chartRangeMax : 100, width : '100%', height : '40px', lineColor : '#6f6', fillColor : '#afa' }); 
        $('#' + this.feedId + ' div.requests').sparkline(this.data.requests, { chartRangeMin : 0, chartRangeMax : 100, width : '100%', height : '40px', lineColor : '#66f', fillColor : '#aaf' }); 
    }
    
    this.pushData = function(list, value)
    {
        list.push(value);
        if (list.length > this.numPoints)
            list.shift();
    }

    // Create feed markup
    var html = '';
    html += '<div id="' + this.feedId + '">\n';
    html += '<h2>' + this.feedName + '</h2>\n';
    html += '<h3>CPU</h3>\n<div class="cpu"></div>\n';
    html += '<h3>Memory</h3><div class="memory"></div>\n'
    html += '<h3>Requests</h3><div class="requests"></div>\n'
    html += '</div>\n';
    $('#' + targetDivId).append(html);
    
    // Populate blank data
    for (var i = 0; i < this.numPoints; i ++)
    {
        this.data.cpu.push(0);
        this.data.memory.push(0);
        this.data.requests.push(0);
    }
    
    this.draw();
    
    // Start timer
    this.timer = setInterval(createDelegate(this, this.fetchData), 1000);
}
