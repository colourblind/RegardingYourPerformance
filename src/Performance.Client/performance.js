function DataFeed(targetDivId, feedUrl, feedName)
{
    this.feedUrl = feedUrl;
    this.feedName = feedName;
    this.feedId = this.feedName.replace(/ /g, '_');
    this.data = {
        cpu : [],
        memory : [],
        requests : []
    };
    this.numPoints = 40;
    
    this.fetchData = function() {
        $.ajax({
            type : 'GET',
            url : this.feedUrl,
            dataType : 'jsonp',
            success : createDelegate(this, this.update),
            error : function(responseData) { alert(responseData); }
        });
    };
    
    this.update = function(responseData)
    {
        this.data.cpu.push(responseData.CpuUsage); 
        if (this.data.cpu.length > this.numPoints) 
            this.data.cpu.splice(0, 1); 
            
        this.data.memory.push(responseData.FreeMemory);
        if (this.data.memory.length > this.numPoints)
            this.data.memory.splice(0, 1);
            
        this.data.requests.push(responseData.RequestsPerSecond);
        if (this.data.requests.length > this.numPoints)
            this.data.requests.splice(0, 1);
        
        $('#' + this.feedId + ' div.cpu').sparkline(this.data.cpu, { chartRangeMin : 0, chartRangeMax : 100, width : '250px', height : '40px', lineColor : '#f66', fillColor : '#faa' }); 
        $('#' + this.feedId + ' div.memory').sparkline(this.data.memory, { chartRangeMin : 0, chartRangeMax : 4000, width : '250px', height : '40px', lineColor : '#6f6', fillColor : '#afa' }); 
        $('#' + this.feedId + ' div.requests').sparkline(this.data.requests, { chartRangeMin : 0, chartRangeMax : 100, width : '250px', height : '40px', lineColor : '#66f', fillColor : '#aaf' }); 
    };

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
    
    // Start timer
    this.timer = setInterval(createDelegate(this, this.fetchData), 1000);
}
